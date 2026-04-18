using System.Collections;
using UnityEngine;

public class MusicManager : Singleton<MusicManager>
{
    [SerializeField] AudioSource mainSource;
    [SerializeField] AudioSource finallySource;

    private Coroutine mainFade;
    private Coroutine finallyFade;

    private void Start()
    {
        SetVolume(SourceType.Main, 0.1f, 2f);
    }

    public void SetVolume(SourceType type, float targetVolume, float time = 0.2f)
    {
        AudioSource source = GetSource(type);
        ref Coroutine previousCoroutine = ref GetPreviousCoroutine(type);

        if (previousCoroutine != null) StopCoroutine(previousCoroutine);
        previousCoroutine = StartCoroutine(FadeVolume(source, targetVolume, time));
    }

    private AudioSource GetSource(SourceType type)
    {
        return type == SourceType.Finally ? finallySource : mainSource;
    }

    private ref Coroutine GetPreviousCoroutine(SourceType type)
    {
        if (type == SourceType.Finally)
            return ref finallyFade;

        return ref mainFade;
    }

    private IEnumerator FadeVolume(AudioSource source, float target, float duration)
    {
        float start = source.volume;
        float elapsed = 0f;

        if (duration <= 0f)
        {
            source.volume = target;
            yield break;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            source.volume = Mathf.Lerp(start, target, t);
            yield return null;
        }

        source.volume = target;
    }
}

public enum SourceType
{
    Main,
    Finally
}