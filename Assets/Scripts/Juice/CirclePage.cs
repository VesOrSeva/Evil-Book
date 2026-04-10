using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;

public class CirclePage : MonoBehaviour
{
    [SerializeField] Volume volume;
    [SerializeField] float duration = 1.5f;
    [SerializeField] float decayMultiplier = 3f;

    private void Start()
    {
        volume.weight = 1f;
        StartCoroutine(ZoomIn());
    }

    private IEnumerator ZoomIn()
    {
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            float shaped = 1f - Mathf.Pow(t, decayMultiplier);

            volume.weight = shaped;

            yield return null;
        }

        volume.weight = 0f;
    }
}