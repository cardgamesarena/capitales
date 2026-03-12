using UnityEngine;

/// <summary>
/// Feedback sonore généré procéduralement (pas besoin de fichiers audio).
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioFeedback : MonoBehaviour
{
    public static AudioFeedback Instance { get; private set; }

    private AudioSource _src;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        _src = GetComponent<AudioSource>();
    }

    public void PlayCorrect()  => StartCoroutine(PlayTone(880f, 0.15f, 0.1f));
    public void PlayWrong()    => StartCoroutine(PlayNoise(0.2f));
    public void PlayClick()    => StartCoroutine(PlayTone(440f, 0.05f, 0.05f));

    System.Collections.IEnumerator PlayTone(float freq, float duration, float vol)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        var clip = AudioClip.Create("tone", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration); // enveloppe décroissante
            data[i] = Mathf.Sin(2f * Mathf.PI * freq * t) * env * vol;
        }
        clip.SetData(data, 0);
        _src.PlayOneShot(clip);
        yield return null;
    }

    System.Collections.IEnumerator PlayNoise(float duration)
    {
        int sampleRate = AudioSettings.outputSampleRate;
        int samples = Mathf.RoundToInt(sampleRate * duration);
        var clip = AudioClip.Create("noise", samples, 1, sampleRate, false);
        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / sampleRate;
            float env = 1f - (t / duration);
            // Bruit filtré pour un "buzz"
            data[i] = (Random.value * 2f - 1f) * env * 0.15f;
        }
        clip.SetData(data, 0);
        _src.PlayOneShot(clip);
        yield return null;
    }
}
