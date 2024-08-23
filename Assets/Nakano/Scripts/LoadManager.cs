using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Extensions;

public class LoadManager : MonoBehaviour
{
    AsyncOperation async;

    [SerializeField] private GameObject loadingUI;

    [SerializeField, Header("�Œ���̃��[�h��ʕ\������")] private float lowestLoadTime = 0.3f;
    private bool didLoadComplete = false; // �V�[���ǂݍ��݊���������

    [SerializeField, Header("�t�F�[�h�����鎞��")] private float fadeTime = 0.15f;
    private CanvasGroup canvasGroup;

    private bool didFadeComplete = false;
    /// <summary>
    /// �t�F�[�h����������
    /// </summary>
    public bool DidFadeComplete { get { return didFadeComplete; } private set { didFadeComplete = value; } }
    private bool isFadeIn = false, isFadeOut = false;

    // �V���O���g��
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
    /// �t�F�[�h
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

        // �����̂Ƃ��͑�UI������ł���悤��SetActive��false�ɕς���
        if(canvasGroup.alpha <= 0)
        {
            loadingUI.SetActive(false);
        }
        else loadingUI.SetActive(true);
    }

    public void LoadScene(string sceneName)
    {
        // �t�F�[�h�C���J�n
        isFadeIn = true;

        // �ǂݍ��݊J�n
        StartCoroutine(LoadSceneCoroutine(sceneName));

        // �ǂݍ��݊�����A�V�[���J�ځE�Œ���҂��ăt�F�[�h�A�E�g�J�n
        StartCoroutine(DelayCoroutine(() => { return (didLoadComplete && !isFadeIn); }, 
                                       () =>
                                       {
                                           // �J��
                                           async.allowSceneActivation = true;
                                           // �ҋ@�E�t�F�[�h�A�E�g
                                           StartCoroutine(DelayCoroutine(lowestLoadTime, () => { isFadeOut = true; }));
                                       }));

    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // �V�[���ǂݍ��݊J�n
        async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (async.progress < 0.9f)
        {
            yield return null;
        }

        if (async.progress >= 0.9f)
        {
            //���[�h����
            didLoadComplete = true;
        }
    }
}
