using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{

    [SerializeField]
    private Material fadeMaterial;  // マテリアルの参照

    [SerializeField]
    private float fadeDuration = 2f;  // フェード効果の時間（秒）

    private void Start()
    {
        if (fadeMaterial == null)
        {
            Debug.LogError("FadeMaterial が設定されていません。");
        }
    }

    public void StartFadeOut()
    {
        if (fadeMaterial != null)
        {
            StartCoroutine(FadeRoutine(true));
        }
    }

    public void StartFadeIn()
    {
        if (fadeMaterial != null)
        {
            StartCoroutine(FadeRoutine(false));
        }
    }

    private IEnumerator FadeRoutine(bool fadeOut)
    {
        // フェード開始時の初期値を設定
        float startAlpha = fadeOut ? 0 : 0.8f;
        float endAlpha = fadeOut ? 0.8f : 0;

        Color color = fadeMaterial.color;
        color.a = startAlpha;
        fadeMaterial.color = color;

        float elapsedTime = 0f;

        // フェードループ
        while (elapsedTime < fadeDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            color.a = alpha;
            fadeMaterial.color = color;
        }

        // 最終不透明度を確保
        color.a = endAlpha;
        fadeMaterial.color = color;
    }
}