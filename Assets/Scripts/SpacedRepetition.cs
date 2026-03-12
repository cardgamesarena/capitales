using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardRecord
{
    public string key;          // "country_capital"
    public int repetitions;     // nombre de fois réussies de suite
    public float easeFactor;    // facteur de facilité (2.5 par défaut)
    public int intervalDays;    // intervalle avant prochaine révision
    public long nextReviewTicks; // DateTime.Ticks pour la prochaine révision
    public int totalAsked;
    public int totalCorrect;

    public CardRecord(string key)
    {
        this.key = key;
        repetitions = 0;
        easeFactor = 2.5f;
        intervalDays = 0;
        nextReviewTicks = DateTime.UtcNow.Ticks;
        totalAsked = 0;
        totalCorrect = 0;
    }

    public DateTime NextReview => new DateTime(nextReviewTicks, DateTimeKind.Utc);
    public bool IsDue => DateTime.UtcNow >= NextReview;
    public float SuccessRate => totalAsked == 0 ? 0f : (float)totalCorrect / totalAsked;
}

// Qualités de réponse (comme SM-2)
public enum AnswerQuality
{
    Blank = 0,      // aucune idée
    Wrong = 1,      // mauvaise réponse
    Hard = 3,       // bonne mais difficile
    Good = 4,       // bonne réponse
    Easy = 5        // très facile
}

public static class SpacedRepetitionSystem
{
    private const string SavePrefix = "srs_";
    private const string KeyListKey = "srs_keys";

    // Applique l'algorithme SM-2 et sauvegarde
    public static void ProcessAnswer(CardRecord card, AnswerQuality quality)
    {
        int q = (int)quality;
        card.totalAsked++;
        if (q >= 3) card.totalCorrect++;

        if (q < 3)
        {
            // Mauvaise réponse : on recommence depuis le début
            card.repetitions = 0;
            card.intervalDays = 1;
        }
        else
        {
            if (card.repetitions == 0)
                card.intervalDays = 1;
            else if (card.repetitions == 1)
                card.intervalDays = 6;
            else
                card.intervalDays = Mathf.RoundToInt(card.intervalDays * card.easeFactor);

            card.repetitions++;
        }

        // Mise à jour du facteur de facilité
        card.easeFactor += (0.1f - (5 - q) * (0.08f + (5 - q) * 0.02f));
        if (card.easeFactor < 1.3f) card.easeFactor = 1.3f;

        // Prochaine révision
        card.nextReviewTicks = DateTime.UtcNow.AddDays(card.intervalDays).Ticks;

        SaveCard(card);
    }

    public static CardRecord GetOrCreateCard(string country)
    {
        string saveKey = SavePrefix + country;
        if (PlayerPrefs.HasKey(saveKey))
        {
            string json = PlayerPrefs.GetString(saveKey);
            return JsonUtility.FromJson<CardRecord>(json);
        }
        return new CardRecord(country);
    }

    public static void SaveCard(CardRecord card)
    {
        string saveKey = SavePrefix + card.key;
        PlayerPrefs.SetString(saveKey, JsonUtility.ToJson(card));

        // Ajouter la clé à la liste si elle n'y est pas
        string keyList = PlayerPrefs.GetString(KeyListKey, "");
        if (!keyList.Contains(card.key + ";"))
        {
            PlayerPrefs.SetString(KeyListKey, keyList + card.key + ";");
        }
        PlayerPrefs.Save();
    }

    // Retourne la prochaine carte à réviser avec priorité intelligente
    public static Capital GetNextCard(List<Capital> allCapitals)
    {
        Capital bestDue = null;
        Capital bestNew = null;
        float lowestScore = float.MaxValue;
        long earliestDue = long.MaxValue;

        foreach (var cap in allCapitals)
        {
            var record = GetOrCreateCard(cap.country);

            if (record.totalAsked == 0)
            {
                // Nouvelle carte jamais vue
                if (bestNew == null) bestNew = cap;
            }
            else if (record.IsDue)
            {
                // Carte due : priorité aux plus en retard et moins bien maîtrisées
                float score = record.SuccessRate * 1000 + (record.nextReviewTicks / TimeSpan.TicksPerMinute);
                if (score < lowestScore)
                {
                    lowestScore = score;
                    bestDue = cap;
                }
            }
        }

        // 70% chance de prendre une carte due si disponible, sinon nouvelle
        if (bestDue != null && bestNew != null)
        {
            return UnityEngine.Random.value < 0.7f ? bestDue : bestNew;
        }
        return bestDue ?? bestNew ?? allCapitals[UnityEngine.Random.Range(0, allCapitals.Count)];
    }

    public static void ResetAll()
    {
        string keyList = PlayerPrefs.GetString(KeyListKey, "");
        foreach (string key in keyList.Split(';'))
        {
            if (!string.IsNullOrEmpty(key))
                PlayerPrefs.DeleteKey(SavePrefix + key);
        }
        PlayerPrefs.DeleteKey(KeyListKey);
        PlayerPrefs.Save();
    }

    public static (int total, int mastered, int learning, int newCards) GetStats(List<Capital> allCapitals)
    {
        int mastered = 0, learning = 0, newCards = 0;
        foreach (var cap in allCapitals)
        {
            var r = GetOrCreateCard(cap.country);
            if (r.totalAsked == 0) newCards++;
            else if (r.repetitions >= 3 && r.SuccessRate >= 0.8f) mastered++;
            else learning++;
        }
        return (allCapitals.Count, mastered, learning, newCards);
    }
}
