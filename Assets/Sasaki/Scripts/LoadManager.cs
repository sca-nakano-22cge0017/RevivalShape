using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions;

public class LoadManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI;

    [SerializeField, Header("�t�F�[�h�����鎞��")] private float fadeTime = 0.15f;
    private CanvasGroup img;

    private bool fadeCheck = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }

    void fade()
    {
        //UI���t�F�[�h������
        if (!fadeCheck)
        {
            img.alpha += fadeTime * Time.deltaTime;
            if (img.alpha == 1)
            {
                fadeCheck = true;
            }
        }
        else
        {
            img.alpha -= fadeTime * Time.deltaTime;
        }
    }
        // �t�F�[�h�C��
        //���[�h��ʏo��
        //loadingUI.SetActive(true);
        //���[�h���I�������
        //�t�F�[�h�A�E�g
        //�I���������

    public static IEnumerator LoadSceneCoroutine(string sceneName)
    {

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);



        while (!async.isDone)
        {
            yield return null;
        }
    }
}
