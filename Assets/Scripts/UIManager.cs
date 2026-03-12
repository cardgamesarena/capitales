using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Panneaux")]
    public GameObject panelStats;
    public GameObject panelQuestion;
    public GameObject panelResult;

    [Header("Stats Panel")]
    public TextMeshProUGUI txtStatsTitle;
    public TextMeshProUGUI txtStatsMastered;
    public TextMeshProUGUI txtStatsLearning;
    public TextMeshProUGUI txtStatsNew;
    public TextMeshProUGUI txtStatsSession;
    public TextMeshProUGUI txtProgressBar;
    public Slider sliderProgress;
    public TMP_Dropdown dropdownContinent;
    public Button btnStart;
    public Button btnReset;

    [Header("Question Panel")]
    public TextMeshProUGUI txtQuestion;
    public TextMeshProUGUI txtProgress;
    public List<Button> choiceButtons;
    public List<TextMeshProUGUI> choiceTexts;
    public Button btnStats;

    [Header("Result Panel")]
    public TextMeshProUGUI txtResultTitle;
    public TextMeshProUGUI txtResultCountry;
    public TextMeshProUGUI txtResultCorrect;
    public TextMeshProUGUI txtResultInfo;
    public Button btnNext;
    public Button btnResultStats;

    [Header("Couleurs")]
    public Color colorCorrect  = new Color(0.063f, 0.725f, 0.506f);  // #10B981 emeraude
    public Color colorWrong    = new Color(0.937f, 0.267f, 0.267f);  // #EF4444 rouge
    public Color colorNeutral  = new Color(0.22f, 0.18f, 0.55f, 1f); // indigo foncé
    public Color colorDisabled = new Color(0.15f, 0.18f, 0.28f, 1f); // sombre

    private string[] continents = { "Tous", "Europe", "Amérique", "Asie", "Afrique", "Océanie" };

    void Start()
    {
        SetupDropdown();
        SetupButtons();
    }

    void SetupDropdown()
    {
        if (dropdownContinent == null) return;
        dropdownContinent.ClearOptions();
        dropdownContinent.AddOptions(new List<string>(continents));
        dropdownContinent.onValueChanged.AddListener(idx => {
            QuizManager.Instance.SetContinentFilter(continents[idx]);
        });
    }

    void SetupButtons()
    {
        if (btnStart) btnStart.onClick.AddListener(() => QuizManager.Instance.StartNewQuestion());
        if (btnReset) btnReset.onClick.AddListener(ConfirmReset);
        if (btnNext) btnNext.onClick.AddListener(() => QuizManager.Instance.StartNewQuestion());
        if (btnResultStats) btnResultStats.onClick.AddListener(() => QuizManager.Instance.ShowStats());
        if (btnStats) btnStats.onClick.AddListener(() => QuizManager.Instance.ShowStats());

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            int idx = i;
            choiceButtons[i].onClick.AddListener(() => OnChoiceClicked(idx));
        }
    }

    void ConfirmReset()
    {
        // Sur mobile/web on fait une confirmation simple via la logique de jeu
        QuizManager.Instance.ResetProgress();
    }

    void HideAll()
    {
        if (panelStats) panelStats.SetActive(false);
        if (panelQuestion) panelQuestion.SetActive(false);
        if (panelResult) panelResult.SetActive(false);
    }

    public void ShowStats(int total, int mastered, int learning, int newCards, int sessionScore, int sessionTotal)
    {
        HideAll();
        if (panelStats) panelStats.SetActive(true);

        if (txtStatsTitle) txtStatsTitle.text = "🌍  Capitales du Monde";
        // Stat cards : afficher juste le nombre (le label est fixe dans la carte)
        if (txtStatsMastered) txtStatsMastered.text = mastered.ToString();
        if (txtStatsLearning) txtStatsLearning.text = learning.ToString();
        if (txtStatsNew) txtStatsNew.text = newCards.ToString();

        string sessionText = sessionTotal > 0
            ? $"Session : {sessionScore}/{sessionTotal}  ({Mathf.RoundToInt(100f * sessionScore / sessionTotal)}%)"
            : "Session : --";
        if (txtStatsSession) txtStatsSession.text = sessionText;

        float progress = total > 0 ? (float)mastered / total : 0f;
        if (sliderProgress) sliderProgress.value = progress;
        if (txtProgressBar) txtProgressBar.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }

    public void ShowQuestion(string country, List<string> choices)
    {
        HideAll();
        if (panelQuestion) panelQuestion.SetActive(true);

        // "Quelle est la capitale de" est affiché par TxtSub (statique dans la carte)
        if (txtQuestion) txtQuestion.text = country;

        var record = QuizManager.Instance.GetCurrentCardRecord();
        if (txtProgress && record != null)
        {
            string badge = record.totalAsked == 0 ? "🆕 Nouveau" :
                record.IsDue ? "⏰ En retard" :
                $"📅 Prochain dans {record.intervalDays}j";
            string pct = record.totalAsked > 0 ? $"  ·  ✓ {Mathf.RoundToInt(record.SuccessRate * 100)}%" : "";
            txtProgress.text = badge + pct;
        }

        for (int i = 0; i < choiceButtons.Count; i++)
        {
            bool hasChoice = i < choices.Count;
            choiceButtons[i].gameObject.SetActive(hasChoice);
            if (hasChoice)
            {
                choiceTexts[i].text = choices[i];
                SetButtonColor(choiceButtons[i], colorNeutral);
                choiceButtons[i].interactable = true;
            }
        }
    }

    void OnChoiceClicked(int idx)
    {
        if (idx >= QuizManager.Instance.CurrentChoices.Count) return;
        string answer = QuizManager.Instance.CurrentChoices[idx];

        // Désactiver tous les boutons immédiatement
        foreach (var btn in choiceButtons) btn.interactable = false;

        // Colorier correct/faux avant de passer au résultat
        string correct = QuizManager.Instance.CurrentCapital.capital;
        for (int i = 0; i < choiceButtons.Count; i++)
        {
            if (i >= QuizManager.Instance.CurrentChoices.Count) continue;
            if (QuizManager.Instance.CurrentChoices[i] == correct)
                SetButtonColor(choiceButtons[i], colorCorrect);
            else if (i == idx)
                SetButtonColor(choiceButtons[i], colorWrong);
            else
                SetButtonColor(choiceButtons[i], colorDisabled);
        }

        StartCoroutine(DelayedSubmit(answer, 0.8f));
    }

    IEnumerator DelayedSubmit(string answer, float delay)
    {
        yield return new WaitForSeconds(delay);
        QuizManager.Instance.SubmitAnswer(answer);
    }

    public void ShowResult(bool correct, string country, string capital)
    {
        HideAll();
        if (panelResult) panelResult.SetActive(true);

        if (txtResultTitle)
        {
            txtResultTitle.text  = correct ? "✓" : "✗";
            txtResultTitle.color = correct ? colorCorrect : colorWrong;
        }
        if (txtResultCountry) txtResultCountry.text = correct ? $"Bravo !  {country}" : country;
        if (txtResultCorrect) txtResultCorrect.text = correct
            ? $"Capitale : <b>{capital}</b>"
            : $"La bonne réponse était : <b>{capital}</b>";

        var record = SpacedRepetitionSystem.GetOrCreateCard(country);
        if (txtResultInfo)
        {
            string nextReview = record.intervalDays <= 1 ? "demain" : $"dans {record.intervalDays} jours";
            txtResultInfo.text = $"📅 Prochaine révision {nextReview}\n" +
                                 $"📊 Score : {record.totalCorrect}/{record.totalAsked} " +
                                 $"({Mathf.RoundToInt(record.SuccessRate * 100)}%)";
        }
    }

    void SetButtonColor(Button btn, Color color)
    {
        var img = btn.GetComponent<UnityEngine.UI.Image>();
        if (img) img.color = color;

        var colors = btn.colors;
        colors.normalColor      = color;
        colors.highlightedColor = new Color(
            Mathf.Min(color.r + 0.1f, 1f),
            Mathf.Min(color.g + 0.1f, 1f),
            Mathf.Min(color.b + 0.1f, 1f), 1f);
        colors.pressedColor  = color * 0.85f;
        colors.disabledColor = colorDisabled;
        colors.fadeDuration  = 0.08f;
        btn.colors = colors;
    }
}
