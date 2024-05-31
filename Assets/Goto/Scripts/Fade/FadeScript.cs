using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{
    [SerializeField]
    private Material fadeMaterial;  // �}�e���A���̎Q��

    [SerializeField]
    private float fadeDuration = 0.5f;  // �t�F�[�h���ʂ̎��ԁi�b�j

    private bool isFading = false;  // �t�F�[�h�����ǂ������m�F����t���O

    private void Start()
    {
        if (fadeMaterial == null)
        {
            Debug.LogError("FadeMaterial ���ݒ肳��Ă��܂���B");
        }
        else
        {
            // ������Ԃ𓧖��ɐݒ�
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

        // �t�F�[�h�J�n���̏����l��ݒ�
        float startAlpha = fadeOut ? 0 : 0.8f;
        float endAlpha = fadeOut ? 0.8f : 0;

        float elapsedTime = 0f;

        // �t�F�[�h���[�v
        while (elapsedTime < fadeDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            SetMaterialAlpha(alpha);
        }

        // �ŏI�s�����x���m��
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