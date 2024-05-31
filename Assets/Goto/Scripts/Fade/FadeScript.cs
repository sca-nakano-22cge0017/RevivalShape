using System.Collections;
using UnityEngine;

public class FadeScript : MonoBehaviour
{

    [SerializeField]
    private Material fadeMaterial;  // �}�e���A���̎Q��

    [SerializeField]
    private float fadeDuration = 2f;  // �t�F�[�h���ʂ̎��ԁi�b�j

    private void Start()
    {
        if (fadeMaterial == null)
        {
            Debug.LogError("FadeMaterial ���ݒ肳��Ă��܂���B");
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
        // �t�F�[�h�J�n���̏����l��ݒ�
        float startAlpha = fadeOut ? 0 : 0.8f;
        float endAlpha = fadeOut ? 0.8f : 0;

        Color color = fadeMaterial.color;
        color.a = startAlpha;
        fadeMaterial.color = color;

        float elapsedTime = 0f;

        // �t�F�[�h���[�v
        while (elapsedTime < fadeDuration)
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
            color.a = alpha;
            fadeMaterial.color = color;
        }

        // �ŏI�s�����x���m��
        color.a = endAlpha;
        fadeMaterial.color = color;
    }
}