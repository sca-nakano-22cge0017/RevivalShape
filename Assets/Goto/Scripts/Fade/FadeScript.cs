using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    [SerializeField]
    private Material fadeMaterial;  // マテリアルの参照

    [SerializeField]
    private float fadeDuration = 0.5f;  // フェード効果の時間（秒）

    private bool isFading = false;  // フェード中かどうかを確認するフラグ

    private void Start()
    {
        if (fadeMaterial == null)
        {
            Debug.LogError("FadeMaterial が設定されていません。");
        }
        else
        {
            // 初期状態を透明に設定
            SetMaterialAlpha(0);
        }
    }

    public void StartFadeOut()
    {
        if (fadeMaterial != null && !isFading)
        {
            StartCoroutine(FadeRoutine(true));
        }
    }

    public void StartFadeIn()
    {
        if (fadeMaterial != null && !isFading)
        {
            StartCoroutine(FadeRoutine(false));
        }
    }

    private IEnumerator FadeRoutine(bool fadeOut)
    {
        isFading = true;

        // フェード開始時の初期値を設定
        float startAlpha = fadeOut ? 0 : 0.8f;
        float endAlpha = fadeOut ? 0.8f : 0;

        float elapsedTime = 0f;

        // フェードループ
        while (elapsedTime < fadeDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            SetMaterialAlpha(alpha);
        }

        // 最終不透明度を確保
        SetMaterialAlpha(endAlpha);

        isFading = false;
    }

    private void SetMaterialAlpha(float alpha)
    {
        if (fadeMaterial != null)
        {
            Color color = fadeMaterial.color;
            color.a = alpha;
            fadeMaterial.color = color;
        }
    }
}