using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class FadeController : MonoBehaviour
{
   public CanvasGroup canvasGroup;
    public float fadeDuration = 1.0f;

    private void Awake()
    {
       
    }

    public void FadeIn()
    {
        StartCoroutine(Fade(0, 1));
    }

    public void FadeOut()
    {
        StartCoroutine(Fade(1, 0));
    }

    private System.Collections.IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsedTime = 0.0f;

        while (elapsedTime < fadeDuration)
        {
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = endAlpha;
    }
}