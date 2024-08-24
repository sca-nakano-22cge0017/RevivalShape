using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions;

public class LoadManager : MonoBehaviour
{
    AsyncOperation async;

    [SerializeField] private GameObject loadingUI;

    private bool isLoading = false;
    public bool IsLoading { get { return isLoading; } private set { isLoading = value; } }

    [SerializeField, Header("最低限のロード画面表示時間")] private float lowestLoadTime;

    [SerializeField, Header("フェードさせる時間")] private float fadeTime;
    private CanvasGroup canvasGroup;

    private bool didFadeComplete = false;
    /// <summary>
    /// フェード完了したか
    /// </summary>
    public bool DidFadeComplete { get { return didFadeComplete; } private set { didFadeComplete = value; } }
    private bool isFadeIn = false, isFadeOut = false;

    // シングルトン
    public static LoadManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        DontDestroyOnLoad(this);

        canvasGroup = loadingUI.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0;
        loadingUI.SetActive(false);
    }

    void Update()
    {
        if(isFadeIn || isFadeOut) Fade();
    }

    /// <summary>
    /// フェード
    /// </summary>
    void Fade()
    {
        if(isFadeIn)
        {
            if (canvasGroup.alpha >= 1)
            {
                canvasGroup.alpha = 1;
                isFadeIn = false;
            }
            else canvasGroup.alpha += Time.deltaTime / fadeTime;
        }

        else if (isFadeOut)
        {
            if (canvasGroup.alpha <= 0)
            {
                canvasGroup.alpha = 0;
                didFadeComplete = true;
                isFadeOut = false;
            }
            else canvasGroup.alpha -= Time.deltaTime / fadeTime;
        }

        // 透明のときは他UIが操作できるようにSetActiveをfalseに変える
        if(canvasGroup.alpha <= 0)
        {
            loadingUI.SetActive(false);
        }
        else loadingUI.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        // 既にロード中なら実行しない
        if (isLoading) return;

        isLoading = true;

        // フェードイン開始
        isFadeIn = true;

        // 読み込み開始
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // フェードイン終わってからシーンロード
        yield return new WaitUntil(() => !isFadeIn);

        // シーン読み込み開始
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return null;
        }
        //ロード完了

        // 遷移
        async.allowSceneActivation = true;

        // 最低限待ってからフェードアウト
        yield return new WaitForSeconds(lowestLoadTime);

        // フェードアウト
        isFadeOut = true;

        isLoading = false;
    }
}
