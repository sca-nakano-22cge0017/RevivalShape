using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions;

public class LoadManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI;

    [SerializeField, Header("フェードさせる時間")] private float fadeTime = 0.15f;
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
        //UIをフェードさせる
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
        // フェードイン
        //ロード画面出す
        //loadingUI.SetActive(true);
        //ロードが終わったら
        //フェードアウト
        //終わった判定

    public static IEnumerator LoadSceneCoroutine(string sceneName)
    {

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);



        while (!async.isDone)
        {
            yield return null;
        }
    }
}
