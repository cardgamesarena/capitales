#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Outil éditeur : crée automatiquement toute la scène UI du quiz.
/// Menu : Tools > Build Quiz Scene
/// Design : Modern Dark Theme
/// </summary>
public class SceneBuilder : MonoBehaviour
{
    // ──────────────────────────────────────────────
    // PALETTE MODERNE
    // ──────────────────────────────────────────────
    static Color BG         = Hex("080C18");
    static Color BG_CARD    = Hex("0F1629");
    static Color BG_CARD2   = Hex("182035");
    static Color BG_CARD3   = Hex("1E2A42");
    static Color BORDER     = Hex("2A3655");
    static Color ACCENT     = Hex("6366F1");   // Indigo
    static Color ACCENT_DIM = Hex("3730A3");   // Indigo sombre
    static Color SUCCESS    = Hex("10B981");   // Emeraude
    static Color SUCCESS_DIM= Hex("064E3B");
    static Color DANGER     = Hex("EF4444");
    static Color DANGER_DIM = Hex("7F1D1D");
    static Color WARN       = Hex("F59E0B");
    static Color TEAL       = Hex("2DD4BF");
    static Color TEAL_DIM   = Hex("134E4A");
    static Color ROSE       = Hex("FB7185");
    static Color ROSE_DIM   = Hex("881337");
    static Color TEXT       = Hex("F1F5F9");
    static Color TEXT2      = Hex("94A3B8");
    static Color TEXT3      = Hex("CBD5E1");
    static Color INDIGO_L   = Hex("C7D2FE");

    static Color Hex(string h)
    {
        ColorUtility.TryParseHtmlString("#" + h, out Color c);
        return c;
    }

    // Sprite arrondi par défaut Unity (pour boutons avec bords arrondis)
    static Sprite RoundRect =>
        AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    // Pas de font custom : on utilise LiberationSans SDF (TMP default, seul fiable en batch WebGL)

    // ──────────────────────────────────────────────
    // ENTRY POINT
    // ──────────────────────────────────────────────
    [MenuItem("Tools/Build Quiz Scene")]
    public static void BuildScene()
    {
        foreach (var go in Object.FindObjectsOfType<GameObject>())
            if (go.transform.parent == null && go.name != "Main Camera")
                Object.DestroyImmediate(go);

        // Canvas
        var canvasGO = new GameObject("Canvas");
        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGO.AddComponent<GraphicRaycaster>();

        // Fond multi-couche (effet de profondeur)
        var bg = MakeImage(canvasGO.transform, "Background", BG);
        Stretch(bg.GetComponent<RectTransform>());

        // Overlay gradient subtil (haut -> transparent)
        var gradTop = MakeImage(canvasGO.transform, "GradTop", new Color(0.25f, 0.2f, 0.55f, 0.12f));
        var gradRT = gradTop.GetComponent<RectTransform>();
        gradRT.anchorMin = new Vector2(0, 0.6f);
        gradRT.anchorMax = Vector2.one;
        gradRT.offsetMin = gradRT.offsetMax = Vector2.zero;

        // EventSystem
        var esGO = new GameObject("EventSystem");
        esGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
        esGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // Panels
        var panelStats    = BuildStatsPanel(canvasGO.transform);
        var panelQuestion = BuildQuestionPanel(canvasGO.transform);
        var panelResult   = BuildResultPanel(canvasGO.transform);

        // Managers
        var mgrsGO  = new GameObject("Managers");
        var uiMgr   = mgrsGO.AddComponent<UIManager>();

        uiMgr.panelStats    = panelStats.root;
        uiMgr.panelQuestion = panelQuestion.root;
        uiMgr.panelResult   = panelResult.root;

        uiMgr.txtStatsTitle    = panelStats.title;
        uiMgr.txtStatsMastered = panelStats.mastered;
        uiMgr.txtStatsLearning = panelStats.learning;
        uiMgr.txtStatsNew      = panelStats.newCards;
        uiMgr.txtStatsSession  = panelStats.session;
        uiMgr.sliderProgress   = panelStats.slider;
        uiMgr.txtProgressBar   = panelStats.progressLabel;
        uiMgr.dropdownContinent= panelStats.dropdown;
        uiMgr.btnStart         = panelStats.btnStart;
        uiMgr.btnReset         = panelStats.btnReset;

        uiMgr.txtQuestion   = panelQuestion.question;
        uiMgr.txtProgress   = panelQuestion.progress;
        uiMgr.choiceButtons = panelQuestion.choiceButtons;
        uiMgr.choiceTexts   = panelQuestion.choiceTexts;
        uiMgr.btnStats      = panelQuestion.btnStats;

        uiMgr.txtResultTitle   = panelResult.title;
        uiMgr.txtResultCountry = panelResult.country;
        uiMgr.txtResultCorrect = panelResult.correct;
        uiMgr.txtResultInfo    = panelResult.info;
        uiMgr.btnNext          = panelResult.btnNext;
        uiMgr.btnResultStats   = panelResult.btnStats;

        var quizMgr = mgrsGO.AddComponent<QuizManager>();
        quizMgr.uiManager = uiMgr;

        Debug.Log("[SceneBuilder] Scène moderne créée !");
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
        r.root = MakePanel(parent, "PanelStats", new Color(0,0,0,0)); // transparent, fond géré par BG
        Stretch(r.root.GetComponent<RectTransform>());

        var vl = r.root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(48, 48, 60, 60);
        vl.spacing = 24;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // ── HEADER ──────────────────────────────────────────────────────────
        var header = MakeCard(r.root.transform, "Header", BG_CARD, 160);
        {
            var hLayout = header.AddComponent<VerticalLayoutGroup>();
            hLayout.padding = new RectOffset(40, 40, 24, 24);
            hLayout.spacing = 8;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = false;

            // Barre d'accent colorée en haut du header
            var accentStrip = MakeImage(header.transform, "AccentStrip",
                new Color(ACCENT.r, ACCENT.g, ACCENT.b, 1f));
            var stripRT = accentStrip.GetComponent<RectTransform>();
            stripRT.anchorMin = Vector2.zero;
            stripRT.anchorMax = new Vector2(1f, 0f);
            stripRT.offsetMin = new Vector2(0, -5);
            stripRT.offsetMax = new Vector2(0, 0);
            accentStrip.GetComponent<LayoutElement>().ignoreLayout = true;

            r.title = MakeTMP(header.transform, "TxtTitle", "🌍  Capitales du Monde",
                52, FontStyles.Bold, TEXT);
            r.title.GetComponent<LayoutElement>().preferredHeight = 70;
        }

        // ── CARTES STATS ─────────────────────────────────────────────────────
        var statsRow = MakeFlex(r.root.transform, "StatsRow", 130);
        {
            var hl = statsRow.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(0, 0, 0, 0);
            hl.spacing = 16;
            hl.childForceExpandWidth = true;
            hl.childForceExpandHeight = true;

            r.mastered = MakeStatCard(statsRow.transform, "CardMastered",
                "✓", "Maîtrisées", "0", TEAL, TEAL_DIM);
            r.learning = MakeStatCard(statsRow.transform, "CardLearning",
                "◉", "Apprentissage", "0", ACCENT, ACCENT_DIM);
            r.newCards = MakeStatCard(statsRow.transform, "CardNew",
                "★", "Nouvelles", "0", ROSE, ROSE_DIM);
        }

        // ── SESSION ──────────────────────────────────────────────────────────
        var sessionCard = MakeCard(r.root.transform, "SessionCard", BG_CARD, 80);
        {
            var hl = sessionCard.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(28, 28, 16, 16);
            hl.spacing = 12;
            hl.childAlignment = TextAnchor.MiddleCenter;
            hl.childForceExpandHeight = true;
            hl.childForceExpandWidth = false;

            var icon = MakeTMP(sessionCard.transform, "Icon", "⚡", 28, FontStyles.Normal, WARN);
            icon.GetComponent<LayoutElement>().preferredWidth = 40;
            r.session = MakeTMP(sessionCard.transform, "TxtSession", "Session : --", 30, FontStyles.Normal, TEXT3);
            r.session.alignment = TextAlignmentOptions.Left;
            r.session.GetComponent<LayoutElement>().flexibleWidth = 1;
        }

        // ── BARRE DE PROGRESSION ─────────────────────────────────────────────
        var progressCard = MakeCard(r.root.transform, "ProgressCard", BG_CARD, 110);
        {
            var pv = progressCard.AddComponent<VerticalLayoutGroup>();
            pv.padding = new RectOffset(28, 28, 18, 18);
            pv.spacing = 10;
            pv.childForceExpandWidth = true;
            pv.childForceExpandHeight = false;

            var labelRow = MakeFlex(progressCard.transform, "LabelRow", 36);
            {
                var hl = labelRow.AddComponent<HorizontalLayoutGroup>();
                hl.childForceExpandWidth = true;
                hl.childForceExpandHeight = true;

                var lblLeft = MakeTMP(labelRow.transform, "LblProg", "Progression globale", 26, FontStyles.Normal, TEXT2);
                lblLeft.alignment = TextAlignmentOptions.Left;
                lblLeft.GetComponent<LayoutElement>().flexibleWidth = 1;

                r.progressLabel = MakeTMP(labelRow.transform, "TxtPct", "0%", 26, FontStyles.Bold, TEAL);
                r.progressLabel.alignment = TextAlignmentOptions.Right;
                r.progressLabel.GetComponent<LayoutElement>().preferredWidth = 100;
            }

            r.slider = MakeSlider(progressCard.transform, "SliderProgress");
        }

        // ── DIVIDER ──────────────────────────────────────────────────────────
        var divider = MakeImage(r.root.transform, "Divider", BORDER);
        divider.GetComponent<LayoutElement>().preferredHeight = 1;

        // ── DROPDOWN ─────────────────────────────────────────────────────────
        var ddLabel = MakeTMP(r.root.transform, "LblContinent", "Filtrer par continent", 26, FontStyles.Normal, TEXT2);
        ddLabel.alignment = TextAlignmentOptions.Left;
        ddLabel.GetComponent<LayoutElement>().preferredHeight = 36;

        r.dropdown = MakeDropdown(r.root.transform, "DropdownContinent");

        // ── BOUTONS ──────────────────────────────────────────────────────────
        r.btnStart = MakeButton(r.root.transform, "BtnStart", "▶   Commencer", ACCENT, 90);
        r.btnReset = MakeButton(r.root.transform, "BtnReset", "Réinitialiser", BG_CARD3, 65);
        // Petite bordure sur le bouton reset pour le rendre "outlined"
        var resetImg = r.btnReset.GetComponent<Image>();
        resetImg.color = BG_CARD2;

        return r;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // QUESTION PANEL
    // ═══════════════════════════════════════════════════════════════════════
    struct QuestionRefs
    {
        public GameObject root;
        public TextMeshProUGUI question, progress;
        public List<Button> choiceButtons;
        public List<TextMeshProUGUI> choiceTexts;
        public Button btnStats;
    }

    static QuestionRefs BuildQuestionPanel(Transform parent)
    {
        var r = new QuestionRefs();
        r.root = MakePanel(parent, "PanelQuestion", new Color(0,0,0,0));
        Stretch(r.root.GetComponent<RectTransform>());
        r.root.SetActive(false);

        var vl = r.root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(40, 40, 40, 40);
        vl.spacing = 20;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // ── TOP BAR ─────────────────────────────────────────────────────────
        var topBar = MakeFlex(r.root.transform, "TopBar", 70);
        {
            var hl = topBar.AddComponent<HorizontalLayoutGroup>();
            hl.padding = new RectOffset(0, 0, 0, 0);
            hl.spacing = 12;
            hl.childForceExpandHeight = true;
            hl.childForceExpandWidth = false;

            r.progress = MakeTMP(topBar.transform, "TxtProgress", "", 24, FontStyles.Normal, TEXT2);
            r.progress.alignment = TextAlignmentOptions.Left;
            r.progress.GetComponent<LayoutElement>().flexibleWidth = 1;

            r.btnStats = MakeButton(topBar.transform, "BtnStats", "📊 Stats", BG_CARD2, 70);
            r.btnStats.GetComponent<LayoutElement>().preferredWidth = 160;
            r.btnStats.GetComponent<LayoutElement>().preferredHeight = 60;
        }

        // ── CARTE QUESTION ──────────────────────────────────────────────────
        var qCard = MakeCard(r.root.transform, "QuestionCard", BG_CARD, 280);
        {
            var qv = qCard.AddComponent<VerticalLayoutGroup>();
            qv.padding = new RectOffset(36, 36, 32, 32);
            qv.spacing = 16;
            qv.childAlignment = TextAnchor.MiddleCenter;
            qv.childForceExpandWidth = true;
            qv.childForceExpandHeight = false;

            // Bande accent à gauche
            var leftAccent = MakeImage(qCard.transform, "LeftAccent", ACCENT);
            leftAccent.GetComponent<LayoutElement>().ignoreLayout = true;
            var laRT = leftAccent.GetComponent<RectTransform>();
            laRT.anchorMin = Vector2.zero;
            laRT.anchorMax = new Vector2(0, 1);
            laRT.offsetMin = Vector2.zero;
            laRT.offsetMax = new Vector2(5, 0);

            var qSub = MakeTMP(qCard.transform, "TxtSub", "Quelle est la capitale de", 28, FontStyles.Normal, TEXT2);
            qSub.GetComponent<LayoutElement>().preferredHeight = 40;

            r.question = MakeTMP(qCard.transform, "TxtQuestion", "", 58, FontStyles.Bold, TEXT);
            r.question.GetComponent<LayoutElement>().preferredHeight = 160;
            r.question.enableWordWrapping = true;
        }

        // ── SPACER ──────────────────────────────────────────────────────────
        var sp1 = MakeFlex(r.root.transform, "Spacer1", 0);
        sp1.GetComponent<LayoutElement>().flexibleHeight = 0.5f;

        // ── BOUTONS DE CHOIX ─────────────────────────────────────────────────
        r.choiceButtons = new List<Button>();
        r.choiceTexts   = new List<TextMeshProUGUI>();

        Color[] choiceColors = {
            new Color(0.22f, 0.18f, 0.55f, 1f),   // Indigo foncé
            new Color(0.08f, 0.35f, 0.35f, 1f),   // Teal foncé
            new Color(0.35f, 0.18f, 0.40f, 1f),   // Violet foncé
            new Color(0.15f, 0.25f, 0.45f, 1f),   // Bleu foncé
        };

        for (int i = 0; i < 4; i++)
        {
            var btn = MakeChoiceButton(r.root.transform, $"BtnChoice{i}", $"Choix {i+1}", choiceColors[i % choiceColors.Length]);
            r.choiceButtons.Add(btn);
            r.choiceTexts.Add(btn.GetComponentInChildren<TextMeshProUGUI>());
        }

        // ── SPACER BAS ───────────────────────────────────────────────────────
        var sp2 = MakeFlex(r.root.transform, "Spacer2", 0);
        sp2.GetComponent<LayoutElement>().flexibleHeight = 1f;

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
        r.root = MakePanel(parent, "PanelResult", new Color(0,0,0,0));
        Stretch(r.root.GetComponent<RectTransform>());
        r.root.SetActive(false);

        var vl = r.root.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(48, 48, 80, 80);
        vl.spacing = 28;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // Spacer haut
        var spTop = MakeFlex(r.root.transform, "SpacerTop", 0);
        spTop.GetComponent<LayoutElement>().flexibleHeight = 1f;

        // ── ICONE RÉSULTAT ───────────────────────────────────────────────────
        var iconGO = MakeFlex(r.root.transform, "ResultIconArea", 140);
        {
            var iconVL = iconGO.AddComponent<VerticalLayoutGroup>();
            iconVL.childAlignment = TextAnchor.MiddleCenter;
            iconVL.childForceExpandWidth = true;
            iconVL.childForceExpandHeight = true;

            // Grand cercle de fond pour l'icone
            var circle = MakeImage(iconGO.transform, "IconCircle", new Color(ACCENT.r, ACCENT.g, ACCENT.b, 0.15f));
            circle.sprite = RoundRect;
            var circleRT = circle.GetComponent<RectTransform>();
            circleRT.anchorMin = new Vector2(0.3f, 0f);
            circleRT.anchorMax = new Vector2(0.7f, 1f);
            circleRT.offsetMin = circleRT.offsetMax = Vector2.zero;
            circle.GetComponent<LayoutElement>().ignoreLayout = true;

            r.title = MakeTMP(iconGO.transform, "TxtTitle", "✓", 100, FontStyles.Bold, SUCCESS);
            r.title.alignment = TextAlignmentOptions.Center;
            r.title.GetComponent<LayoutElement>().preferredHeight = 130;
        }

        // ── CARTE INFO PAYS ──────────────────────────────────────────────────
        var infoCard = MakeCard(r.root.transform, "InfoCard", BG_CARD, 200);
        {
            var iv = infoCard.AddComponent<VerticalLayoutGroup>();
            iv.padding = new RectOffset(36, 36, 28, 28);
            iv.spacing = 12;
            iv.childAlignment = TextAnchor.MiddleCenter;
            iv.childForceExpandWidth = true;
            iv.childForceExpandHeight = false;

            r.country = MakeTMP(infoCard.transform, "TxtCountry", "", 40, FontStyles.Bold, TEXT);
            r.country.GetComponent<LayoutElement>().preferredHeight = 55;

            var divLine = MakeImage(infoCard.transform, "DivLine", BORDER);
            divLine.GetComponent<LayoutElement>().preferredHeight = 1;

            r.correct = MakeTMP(infoCard.transform, "TxtCorrect", "", 34, FontStyles.Normal, TEXT3);
            r.correct.GetComponent<LayoutElement>().preferredHeight = 48;
        }

        // ── INFO RÉVISION ────────────────────────────────────────────────────
        r.info = MakeTMP(r.root.transform, "TxtInfo", "", 26, FontStyles.Normal, TEXT2);
        r.info.GetComponent<LayoutElement>().preferredHeight = 72;

        // ── BOUTONS ──────────────────────────────────────────────────────────
        r.btnNext  = MakeButton(r.root.transform, "BtnNext",  "→   Question suivante", ACCENT, 90);
        r.btnStats = MakeButton(r.root.transform, "BtnStats", "📊  Voir les stats",    BG_CARD2, 70);

        // Spacer bas
        var spBot = MakeFlex(r.root.transform, "SpacerBot", 0);
        spBot.GetComponent<LayoutElement>().flexibleHeight = 1f;

        return r;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // HELPERS DE CONSTRUCTION
    // ═══════════════════════════════════════════════════════════════════════

    // Carte avec fond et LayoutElement
    static GameObject MakeCard(Transform parent, string name, Color color, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = RoundRect;
        img.type = Image.Type.Sliced;
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        return go;
    }

    // Container flex sans fond
    static GameObject MakeFlex(Transform parent, string name, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.AddComponent<RectTransform>();
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;
        return go;
    }

    // Panel de base (fond plein)
    static GameObject MakePanel(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        return go;
    }

    static Image MakeImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        go.AddComponent<LayoutElement>();
        return img;
    }

    // Carte stat individuelle (3 dans une ligne)
    static TextMeshProUGUI MakeStatCard(Transform parent, string name,
        string icon, string label, string value, Color accent, Color bg)
    {
        var card = new GameObject(name);
        card.transform.SetParent(parent, false);
        var img = card.AddComponent<Image>();
        img.color = bg;
        img.sprite = RoundRect;
        img.type = Image.Type.Sliced;
        var le = card.AddComponent<LayoutElement>();
        le.flexibleWidth = 1;

        var vl = card.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(16, 16, 20, 20);
        vl.spacing = 8;
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        var iconTMP = MakeTMP(card.transform, "Icon", icon, 36, FontStyles.Bold, accent);
        iconTMP.GetComponent<LayoutElement>().preferredHeight = 44;

        // Valeur (le champ retourné = celui mis à jour par UIManager)
        var valTMP = MakeTMP(card.transform, "Value", value, 42, FontStyles.Bold, accent);
        valTMP.GetComponent<LayoutElement>().preferredHeight = 52;

        var lbl = MakeTMP(card.transform, "Label", label, 22, FontStyles.Normal, new Color(accent.r, accent.g, accent.b, 0.75f));
        lbl.GetComponent<LayoutElement>().preferredHeight = 32;

        return valTMP; // UIManager mettra à jour cette valeur
    }

    // Bouton choix réponse (avec icone de lettre)
    static Button MakeChoiceButton(Transform parent, string name, string label, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = RoundRect;
        img.type = Image.Type.Sliced;
        var btn = go.AddComponent<Button>();

        var cs = btn.colors;
        cs.normalColor      = color;
        cs.highlightedColor = new Color(color.r + 0.1f, color.g + 0.1f, color.b + 0.1f, 1f);
        cs.pressedColor     = new Color(color.r - 0.05f, color.g - 0.05f, color.b - 0.05f, 1f);
        cs.disabledColor    = new Color(0.15f, 0.15f, 0.2f, 1f);
        cs.fadeDuration     = 0.1f;
        btn.colors = cs;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 90;
        le.preferredHeight = 100;

        // Barre latérale colorée
        var bar = new GameObject("Bar");
        bar.transform.SetParent(go.transform, false);
        var barImg = bar.AddComponent<Image>();
        barImg.color = new Color(1f, 1f, 1f, 0.25f);
        var barRT = bar.GetComponent<RectTransform>();
        barRT.anchorMin = Vector2.zero;
        barRT.anchorMax = new Vector2(0, 1);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = new Vector2(6, 0);

        // Texte
        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = 36;
        tmp.color = TEXT;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Normal;
        tmp.enableWordWrapping = false;
        Stretch(tmp.GetComponent<RectTransform>());

        return btn;
    }

    // Bouton standard
    static Button MakeButton(Transform parent, string name, string label, Color color, float height)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = color;
        img.sprite = RoundRect;
        img.type = Image.Type.Sliced;
        var btn = go.AddComponent<Button>();

        var cs = btn.colors;
        cs.normalColor      = color;
        cs.highlightedColor = new Color(
            Mathf.Min(color.r + 0.12f, 1f),
            Mathf.Min(color.g + 0.12f, 1f),
            Mathf.Min(color.b + 0.12f, 1f), 1f);
        cs.pressedColor = new Color(color.r * 0.85f, color.g * 0.85f, color.b * 0.85f, 1f);
        cs.fadeDuration = 0.1f;
        btn.colors = cs;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = height;
        le.preferredHeight = height;

        var txtGO = new GameObject("Text");
        txtGO.transform.SetParent(go.transform, false);
        var tmp = txtGO.AddComponent<TextMeshProUGUI>();
        tmp.text = label;
        tmp.fontSize = height > 80 ? 36 : 30;
        tmp.color = TEXT;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = FontStyles.Bold;
        Stretch(tmp.GetComponent<RectTransform>());

        return btn;
    }

    static TextMeshProUGUI MakeTMP(Transform parent, string name, string text, float size, FontStyles style, Color color)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.fontStyle = style;
        tmp.enableWordWrapping = true;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = size * 1.4f;
        le.preferredHeight = size * 1.7f;
        return tmp;
    }

    // Slider moderne (épais, coloré)
    static Slider MakeSlider(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var bgGO = new GameObject("Background");
        bgGO.transform.SetParent(go.transform, false);
        var bgImg = bgGO.AddComponent<Image>();
        bgImg.color = BORDER;
        bgImg.sprite = RoundRect;
        bgImg.type = Image.Type.Sliced;
        Stretch(bgImg.GetComponent<RectTransform>());

        var fillAreaGO = new GameObject("Fill Area");
        fillAreaGO.transform.SetParent(go.transform, false);
        var faRT = fillAreaGO.AddComponent<RectTransform>();
        faRT.anchorMin = Vector2.zero;
        faRT.anchorMax = Vector2.one;
        faRT.offsetMin = faRT.offsetMax = Vector2.zero;

        var fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(fillAreaGO.transform, false);
        var fillImg = fillGO.AddComponent<Image>();
        fillImg.color = TEAL;
        fillImg.sprite = RoundRect;
        fillImg.type = Image.Type.Sliced;
        Stretch(fillImg.GetComponent<RectTransform>());

        var slider = go.AddComponent<Slider>();
        slider.fillRect = fillImg.GetComponent<RectTransform>();
        slider.targetGraphic = bgImg;
        slider.interactable = false;
        slider.value = 0;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 14;
        le.preferredHeight = 14;

        return slider;
    }

    // Dropdown moderne
    static TMP_Dropdown MakeDropdown(Transform parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var img = go.AddComponent<Image>();
        img.color = BG_CARD2;
        img.sprite = RoundRect;
        img.type = Image.Type.Sliced;
        var dd = go.AddComponent<TMP_Dropdown>();

        var labelGO = new GameObject("Label");
        labelGO.transform.SetParent(go.transform, false);
        var labelTMP = labelGO.AddComponent<TextMeshProUGUI>();
        labelTMP.text = "Tous les continents";
        labelTMP.fontSize = 30;
        labelTMP.color = TEXT3;
        labelTMP.alignment = TextAlignmentOptions.Center;
        Stretch(labelTMP.GetComponent<RectTransform>());
        dd.captionText = labelTMP;

        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 72;
        le.preferredHeight = 72;

        return dd;
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}
#endif
