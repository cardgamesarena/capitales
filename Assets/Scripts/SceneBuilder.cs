#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;

/// <summary>
/// Outil éditeur : crée automatiquement toute la scène UI du quiz.
/// Menu : Tools > Build Quiz Scene
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    [MenuItem("Tools/Build Quiz Scene")]
    public static void BuildScene()
    {
        // Nettoyage
        foreach (var go in Object.FindObjectsOfType<GameObject>())
            if (go.transform.parent == null && go.name != "Main Camera")
                Object.DestroyImmediate(go);

        // ── Canvas ──────────────────────────────────────────────────────────
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        ((CanvasScaler)canvasGO.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // ── Fond ────────────────────────────────────────────────────────────
        var bg = CreateImage(canvasGO.transform, "Background", new Color(0.08f, 0.1f, 0.18f));
        SetStretch(bg.GetComponent<RectTransform>());

        // ── EventSystem ─────────────────────────────────────────────────────
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // ── Panneaux ────────────────────────────────────────────────────────
        var panelStats = BuildStatsPanel(canvasGO.transform);
        var panelQuestion = BuildQuestionPanel(canvasGO.transform);
        var panelResult = BuildResultPanel(canvasGO.transform);

        // ── Managers ────────────────────────────────────────────────────────
        var mgrsGO = new GameObject("Managers");

        var uiMgr = mgrsGO.AddComponent<UIManager>();
        uiMgr.panelStats    = panelStats.root;
        uiMgr.panelQuestion = panelQuestion.root;
        uiMgr.panelResult   = panelResult.root;

        // Stats refs
        uiMgr.txtStatsTitle   = panelStats.title;
        uiMgr.txtStatsMastered = panelStats.mastered;
        uiMgr.txtStatsLearning = panelStats.learning;
        uiMgr.txtStatsNew     = panelStats.newCards;
        uiMgr.txtStatsSession = panelStats.session;
        uiMgr.sliderProgress  = panelStats.slider;
        uiMgr.txtProgressBar  = panelStats.progressLabel;
        uiMgr.dropdownContinent = panelStats.dropdown;
        uiMgr.btnStart        = panelStats.btnStart;
        uiMgr.btnReset        = panelStats.btnReset;

        // Question refs
        uiMgr.txtQuestion  = panelQuestion.question;
        uiMgr.txtProgress  = panelQuestion.progress;
        uiMgr.choiceButtons = panelQuestion.choiceButtons;
        uiMgr.choiceTexts   = panelQuestion.choiceTexts;
        uiMgr.btnStats      = panelQuestion.btnStats;

        // Result refs
        uiMgr.txtResultTitle   = panelResult.title;
        uiMgr.txtResultCountry = panelResult.country;
        uiMgr.txtResultCorrect = panelResult.correct;
        uiMgr.txtResultInfo    = panelResult.info;
        uiMgr.btnNext          = panelResult.btnNext;
        uiMgr.btnResultStats   = panelResult.btnStats;

        var quizMgr = mgrsGO.AddComponent<QuizManager>();
        quizMgr.uiManager = uiMgr;

        Debug.Log("[SceneBuilder] Scène créée avec succès !");
        Selection.activeGameObject = mgrsGO;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // STATS PANEL
    // ═══════════════════════════════════════════════════════════════════════
    struct StatsRefs
    {
        public GameObject root;
        public TextMeshProUGUI title, mastered, learning, newCards, session, progressLabel;
        public Slider slider;
        public TMP_Dropdown dropdown;
        public Button btnStart, btnReset;
    }

    static StatsRefs BuildStatsPanel(Transform parent)
    {
        var r = new StatsRefs();
        r.root = CreatePanel(parent, "PanelStats", new Color(0.1f, 0.13f, 0.22f, 0.95f));

        var vl = r.root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(60, 60, 80, 80);
        vl.spacing = 30;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        r.title    = CreateTMP(r.root.transform, "TxtTitle", "Capitales du Monde", 52, FontStyle.Bold, Color.white);
        r.mastered = CreateTMP(r.root.transform, "TxtMastered", "Maîtrisées : 0", 36, FontStyle.Normal, new Color(0.2f, 0.9f, 0.4f));
        r.learning = CreateTMP(r.root.transform, "TxtLearning", "En apprentissage : 0", 36, FontStyle.Normal, new Color(1f, 0.8f, 0.2f));
        r.newCards = CreateTMP(r.root.transform, "TxtNew", "Nouvelles : 0", 36, FontStyle.Normal, new Color(0.5f, 0.7f, 1f));
        r.session  = CreateTMP(r.root.transform, "TxtSession", "Session : --", 32, FontStyle.Normal, Color.white);

        // Slider de progression
        r.slider = CreateSlider(r.root.transform, "SliderProgress");
        r.progressLabel = CreateTMP(r.root.transform, "TxtProgress", "0% maîtrisé", 28, FontStyle.Normal, Color.gray);

        // Dropdown continent
        r.dropdown = CreateDropdown(r.root.transform, "DropdownContinent");

        r.btnStart = CreateButton(r.root.transform, "BtnStart", "Commencer", new Color(0.2f, 0.5f, 1f));
        r.btnReset = CreateButton(r.root.transform, "BtnReset", "Réinitialiser", new Color(0.7f, 0.2f, 0.2f));

        return r;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // QUESTION PANEL
    // ═══════════════════════════════════════════════════════════════════════
    struct QuestionRefs
    {
        public GameObject root;
        public TextMeshProUGUI question, progress;
        public System.Collections.Generic.List<Button> choiceButtons;
        public System.Collections.Generic.List<TextMeshProUGUI> choiceTexts;
        public Button btnStats;
    }

    static QuestionRefs BuildQuestionPanel(Transform parent)
    {
        var r = new QuestionRefs();
        r.root = CreatePanel(parent, "PanelQuestion", new Color(0.08f, 0.1f, 0.18f));
        r.root.SetActive(false);

        r.progress = CreateTMP(r.root.transform, "TxtProgress", "", 26, FontStyle.Normal, Color.gray);
        SetAnchors(r.progress.GetComponent<RectTransform>(), new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -50));

        r.question = CreateTMP(r.root.transform, "TxtQuestion", "", 44, FontStyle.Bold, Color.white);
        r.question.alignment = TextAlignmentOptions.Center;
        var qRT = r.question.GetComponent<RectTransform>();
        qRT.anchorMin = new Vector2(0.05f, 0.55f);
        qRT.anchorMax = new Vector2(0.95f, 0.85f);
        qRT.offsetMin = qRT.offsetMax = Vector2.zero;

        r.choiceButtons = new System.Collections.Generic.List<Button>();
        r.choiceTexts   = new System.Collections.Generic.List<TextMeshProUGUI>();

        float[] yPositions = { 0.48f, 0.35f, 0.22f, 0.09f };
        for (int i = 0; i < 4; i++)
        {
            var btn = CreateButton(r.root.transform, $"BtnChoice{i}", $"Choix {i+1}", new Color(0.2f, 0.35f, 0.65f));
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.05f, yPositions[i]);
            rt.anchorMax = new Vector2(0.95f, yPositions[i] + 0.11f);
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            var txt = btn.GetComponentInChildren<TextMeshProUGUI>();
            txt.fontSize = 34;
            r.choiceButtons.Add(btn);
            r.choiceTexts.Add(txt);
        }

        r.btnStats = CreateButton(r.root.transform, "BtnStats", "Stats", new Color(0.3f, 0.3f, 0.5f));
        var statsRT = r.btnStats.GetComponent<RectTransform>();
        statsRT.anchorMin = new Vector2(0.35f, 0.01f);
        statsRT.anchorMax = new Vector2(0.65f, 0.07f);
        statsRT.offsetMin = statsRT.offsetMax = Vector2.zero;

        return r;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // RESULT PANEL
    // ═══════════════════════════════════════════════════════════════════════
    struct ResultRefs
    {
        public GameObject root;
        public TextMeshProUGUI title, country, correct, info;
        public Button btnNext, btnStats;
    }

    static ResultRefs BuildResultPanel(Transform parent)
    {
        var r = new ResultRefs();
        r.root = CreatePanel(parent, "PanelResult", new Color(0.08f, 0.1f, 0.18f));
        r.root.SetActive(false);

        var vl = r.root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(60, 60, 200, 100);
        vl.spacing = 40;
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        r.title   = CreateTMP(r.root.transform, "TxtTitle", "", 64, FontStyle.Bold, Color.white);
        r.country = CreateTMP(r.root.transform, "TxtCountry", "", 42, FontStyle.Normal, Color.white);
        r.correct = CreateTMP(r.root.transform, "TxtCorrect", "", 38, FontStyle.Normal, Color.white);
        r.info    = CreateTMP(r.root.transform, "TxtInfo", "", 30, FontStyle.Normal, Color.gray);
        r.btnNext  = CreateButton(r.root.transform, "BtnNext", "Question suivante", new Color(0.2f, 0.5f, 1f));
        r.btnStats = CreateButton(r.root.transform, "BtnStats", "Voir les stats", new Color(0.3f, 0.3f, 0.5f));

        return r;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════════════════
    static GameObject CreatePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        SetStretch(go.GetComponent<RectTransform>());
        return go;
    }

    static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return img;
    }

    static TextMeshProUGUI CreateTMP(Transform parent, string name, string text, float size, FontStyle style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style == FontStyle.Bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.enableWordWrapping = true;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = size * 1.5f;
        le.preferredHeight = size * 1.8f;
        return tmp;
    }

    static Button CreateButton(Transform parent, string name, string label, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        var btn = go.AddComponent<Button>();

        var cs = btn.colors;
        cs.normalColor   = color;
        cs.highlightedColor = color * 1.15f;
        cs.pressedColor  = color * 0.85f;
        btn.colors = cs;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.color = Color.white;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        SetStretch(tmp.GetComponent<RectTransform>());

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 90;
        le.preferredHeight = 100;

        return btn;
    }

    static Slider CreateSlider(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = new Color(0.2f, 0.2f, 0.3f);
        SetStretch(bgImg.GetComponent<RectTransform>());

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(go.transform, false);
        var fillAreaRT = fillAreaGO.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = Vector2.zero;
        fillAreaRT.anchorMax = Vector2.one;
        fillAreaRT.offsetMin = fillAreaRT.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = new Color(0.2f, 0.7f, 0.4f);
        SetStretch(fillImg.GetComponent<RectTransform>());

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillImg.GetComponent<RectTransform>();
        slider.targetGraphic = bgImg;
        slider.value = 0;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 30;
        le.preferredHeight = 30;

        return slider;
    }

    static TMP_Dropdown CreateDropdown(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = new Color(0.2f, 0.25f, 0.4f);
        var dd = go.AddComponent<TMP_Dropdown>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = "Tous les continents";
        labelTMP.fontSize = 30;
        labelTMP.color = Color.white;
        labelTMP.alignment = TextAlignmentOptions.Center;
        SetStretch(labelTMP.GetComponent<RectTransform>());
        dd.captionText = labelTMP;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 80;
        le.preferredHeight = 80;

        return dd;
    }

    static void SetStretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }

    static void SetAnchors(RectTransform rt, Vector2 min, Vector2 max, Vector2 offset)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = offset;
    }
}
#endif
