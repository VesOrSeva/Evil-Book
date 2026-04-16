using System.Collections;
using UnityEngine;

public class Intro : MonoBehaviour
{
    [SerializeField] CanvasGroup cg;
    [SerializeField] float speed = 3f;

    private void Awake()
    {
        cg.alpha = 1;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(0.2f);

        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < 1f)
        {
            time += Time.deltaTime * speed;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, time);
            yield return null;
        }

        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }
}
