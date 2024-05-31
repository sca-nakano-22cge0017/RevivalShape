using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeSceneLoader : MonoBehaviour
{
    [SerializeField]
    public Image fadePanel;             // �t�F�[�h�p��UI�p�l���iImage�j
    [SerializeField]
    public float fadeDuration = 1.0f;   // �t�F�[�h�̊����ɂ����鎞��
    [SerializeField]
    private GameObject Panel;


    public void CallCoroutine()
    {
        StartCoroutine(FadeOutAndLoadScene());
    }

    public IEnumerator FadeOutAndLoadScene()
    {
        fadePanel.enabled = true;                 // �p�l����L����
        float elapsedTime = 0.0f;                 // �o�ߎ��Ԃ�������
        Color startColor = fadePanel.color;       // �t�F�[�h�p�l���̊J�n�F���擾
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 1.0f); // �t�F�[�h�p�l���̍ŏI�F��ݒ�

        // �t�F�[�h�A�E�g�A�j���[�V���������s
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;                        // �o�ߎ��Ԃ𑝂₷
            float t = Mathf.Clamp01(elapsedTime / fadeDuration);  // �t�F�[�h�̐i�s�x���v�Z
            fadePanel.color = Color.Lerp(startColor, endColor, t); // �p�l���̐F��ύX���ăt�F�[�h�A�E�g
            yield return null;                                     // 1�t���[���ҋ@
        }

        fadePanel.color = endColor;  // �t�F�[�h������������ŏI�F�ɐݒ�
        Panel.SetActive(true);
       // SceneManager.LoadScene("Menu"); // �V�[�������[�h���ă��j���[�V�[���ɑJ��
    }
}