using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum QuizState
{
    Idle,
    Question,
    Result,
    Stats
}

public class QuizManager : MonoBehaviour
{
    public static QuizManager Instance { get; private set; }

    [Header("Références UI")]
    public UIManager uiManager;

    [Header("Paramètres")]
    public int numberOfChoices = 4;
    public string continentFilter = "Tous";

    // État courant
    public QuizState State { get; private set; } = QuizState.Idle;
    public Capital CurrentCapital { get; private set; }
    public List<string> CurrentChoices { get; private set; } = new List<string>();
    public bool LastAnswerCorrect { get; private set; }
    public string LastCorrectAnswer { get; private set; }
    public int SessionScore { get; private set; }
    public int SessionTotal { get; private set; }

    private List<Capital> _filteredCapitals;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        RefreshFilteredList();
        ShowStats();
    }

    public void RefreshFilteredList()
    {
        if (continentFilter == "Tous")
            _filteredCapitals = new List<Capital>(CapitalDatabase.All);
        else
            _filteredCapitals = CapitalDatabase.All.FindAll(c => c.continent == continentFilter);
    }

    public void SetContinentFilter(string continent)
    {
        continentFilter = continent;
        RefreshFilteredList();
    }

    public void StartNewQuestion()
    {
        if (_filteredCapitals == null || _filteredCapitals.Count == 0)
        {
            RefreshFilteredList();
        }

        CurrentCapital = SpacedRepetitionSystem.GetNextCard(_filteredCapitals);
        CurrentChoices = GenerateChoices(CurrentCapital);
        State = QuizState.Question;
        uiManager.ShowQuestion(CurrentCapital.country, CurrentChoices);
    }

    private List<string> GenerateChoices(Capital correct)
    {
        var choices = new List<string> { correct.capital };
        var pool = new List<Capital>(_filteredCapitals);
        pool.RemoveAll(c => c.country == correct.country);

        // Shuffle
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
        }

        int toAdd = Mathf.Min(numberOfChoices - 1, pool.Count);
        for (int i = 0; i < toAdd; i++)
            choices.Add(pool[i].capital);

        // Shuffle final choices
        for (int i = choices.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            string tmp = choices[i]; choices[i] = choices[j]; choices[j] = tmp;
        }

        return choices;
    }

    public void SubmitAnswer(string answer)
    {
        if (State != QuizState.Question) return;

        bool correct = string.Compare(answer.Trim(), CurrentCapital.capital.Trim(),
            System.StringComparison.OrdinalIgnoreCase) == 0;

        LastAnswerCorrect = correct;
        LastCorrectAnswer = CurrentCapital.capital;
        SessionTotal++;

        var record = SpacedRepetitionSystem.GetOrCreateCard(CurrentCapital.country);
        AnswerQuality quality;

        if (correct)
        {
            SessionScore++;
            quality = AnswerQuality.Good;
        }
        else
        {
            quality = AnswerQuality.Wrong;
        }

        SpacedRepetitionSystem.ProcessAnswer(record, quality);

        State = QuizState.Result;
        uiManager.ShowResult(correct, CurrentCapital.country, CurrentCapital.capital);
    }

    public void ShowStats()
    {
        State = QuizState.Stats;
        var (total, mastered, learning, newCards) = SpacedRepetitionSystem.GetStats(_filteredCapitals ?? CapitalDatabase.All);
        uiManager.ShowStats(total, mastered, learning, newCards, SessionScore, SessionTotal);
    }

    public void ResetProgress()
    {
        SpacedRepetitionSystem.ResetAll();
        SessionScore = 0;
        SessionTotal = 0;
        ShowStats();
    }

    public CardRecord GetCurrentCardRecord()
    {
        if (CurrentCapital == null) return null;
        return SpacedRepetitionSystem.GetOrCreateCard(CurrentCapital.country);
    }
}
