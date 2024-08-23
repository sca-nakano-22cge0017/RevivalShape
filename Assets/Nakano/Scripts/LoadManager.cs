using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions;

public class LoadManager : MonoBehaviour
{
    AsyncOperation async;

    [SerializeField] private GameObject loadingUI;

    [SerializeField, Header("最低限のロード画面表示時間")] private float lowestLoadTime = 0.3f;
    private bool didLoadComplete = false; // シーン読み込み完了したか

    [SerializeField, Header("フェードさせる時間")] private float fadeTime = 0.15f;
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
            canvasGroup.alpha += Time.deltaTime / fadeTime;
            if (canvasGroup.alpha >= 1)
            {
                isFadeIn = false;
            }
        }

        else if (isFadeOut)
        {
            canvasGroup.alpha -= Time.deltaTime / fadeTime;
            if (canvasGroup.alpha <= 0)
            {
                didFadeComplete = true;
                isFadeOut = false;
            }
            else didFadeComplete = false;
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
        // フェードイン開始
        isFadeIn = true;

        // 読み込み開始
        StartCoroutine(LoadSceneCoroutine(sceneName));

        // 読み込み完了後、シーン遷移・最低限待ってフェードアウト開始
        StartCoroutine(DelayCoroutine(() => { return (didLoadComplete && !isFadeIn); }, 
                                       () =>
                                       {
                                           // 遷移
                                           async.allowSceneActivation = true;
                                           // 待機・フェードアウト
                                           StartCoroutine(DelayCoroutine(lowestLoadTime, () => { isFadeOut = true; }));
                                       }));

    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // シーン読み込み開始
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return null;
        }

        if (async.progress >= 0.9f)
        {
            //ロード完了
            didLoadComplete = true;
        }
    }
}
