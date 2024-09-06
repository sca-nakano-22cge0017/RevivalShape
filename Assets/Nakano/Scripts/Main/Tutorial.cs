using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using static Extensions;

[System.Serializable]
public class Windows
{
    public GameObject order;
    public GameObject[] objects;
}

/// <summary>
/// チュートリアル制御
/// </summary>
public class Tutorial : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TimeManager timeManager;

    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Windows[] checkPhase;
    [SerializeField] private Windows[] selectPhase;
    [SerializeField] private Windows[] playPhase;

    [SerializeField] float phaseChangeCoolTime = 0.5f;
    [SerializeField, Header("UI操作不可にする用Image")] GameObject obstruct;
    [SerializeField, Header("タップしてから次にタップできるようになるまでの時間")] float tapCoolTime = 0.5f;

    private int tapCount = 0;
    private bool tutorialCompleteByPhase = false;
    /// <summary>
    /// 各フェーズのチュートリアルが終了しているか
    /// </summary>
    public bool TutorialCompleteByPhase { get { return tutorialCompleteByPhase; } private set { tutorialCompleteByPhase = value; } }

    delegate void PlayFunc();
    PlayFunc playFunc;

    private bool isTutorialComplete = false;
    /// <summary>
    ///  チュートリアルが全て終了しているか
    /// </summary>
    public bool IsTutorialComplete { get { return isTutorialComplete; } private set { } }

    private string methodName = "";
    public string MethodName { get { return methodName; } private set { methodName = value; } }

    private bool isCoolTime = false;
    private WaitForSeconds coolTime = new WaitForSeconds(0.1f); // 次のウィンドウ表示までのクールタイム

    // SE
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = FindObjectOfType<SoundManager>();
    }

    public void TutorialStart()
    {
        tutorialCanvas.SetActive(true);
        obstruct.SetActive(true);
        timeManager.OnStop();

        StartCoroutine(DelayCoroutine(phaseChangeCoolTime, () => 
        {
            playFunc = CheckA;
            SEPlay();
        }));
    }

    public void TutorialUpdate()
    {
        if(!stageController || !timeManager) return;

        if (playFunc != null && !isTutorialComplete)
        {
            playFunc();
            methodName = playFunc.Method.Name;
        }

        if (isTutorialComplete)
        {
            obstruct.SetActive(false);
        }
    }

    // 確認フェーズの説明
    void CheckA()
    {
        NextWindowDisplayByTap(checkPhase[0], CheckB, () => 
        {
            StartCoroutine(DelayCoroutine(0.1f, () => 
            {
                // 右移動指示
                if (!checkPhase[1].order.activeSelf)
                {
                    checkPhase[1].order.SetActive(true);
                    checkPhase[1].objects[0].SetActive(true);

                    SEPlay();
                }
            }));

            StartCoroutine(DelayCoroutine(tapCoolTime, () => {
                isCheckB_1 = true;
            }));
        });
    }

    private bool isCheckB_1 = false;
    public bool IsCheckB_1
    {
        get { return isCheckB_1; }
        private set { }
    }

    // 右方向へスワイプしたか
    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { 
        get { return toCheckB_2;} 
        set {
            if(toCheckB_2 || isTutorialComplete) return;
            toCheckB_2 = value;

            StartCoroutine(DelayCoroutine(0.2f, () =>
            {
                isCheckB_2 = true;
            }));

            // 左スワイプ指示
            if (!checkPhase[1].objects[1].activeSelf)
            {
                checkPhase[1].objects[0].SetActive(false);
                checkPhase[1].objects[1].SetActive(true);
            }
        } 
    }

    private bool isCheckB_2 = false;
    public bool IsCheckB_2
    {
        get { return isCheckB_2; }
        private set { isCheckB_2 = value; }
    }

    // ドラッグ操作の説明
    void CheckB()
    {
        if (checkPhase[0].order.activeSelf)
        {
            checkPhase[0].order.SetActive(false);
        }
    }

    // 左方向へスワイプしたか
    private bool toCheckC = false;
    public bool ToCheckC
    {
        get { return toCheckC; }
        set
        {
            if (toCheckC || isTutorialComplete) return;
            toCheckC = true;

            // スワイプ指示非表示
            checkPhase[1].order.SetActive(false);
            checkPhase[1].objects[1].SetActive(false);

            playFunc = CheckC;

            if (!checkPhase[2].order.activeSelf)
            {
                checkPhase[2].order.SetActive(true);
                SEPlay();
            }

            obstruct.SetActive(false);
        }
    }

    // リセットボタンを押したか
    private bool isCheckC = false;
    public bool IsCheckC
    {
        get { return isCheckC; }
        set
        {
            if (isCheckC || isTutorialComplete) return;
            isCheckC = value;

            // リセットボタンが押されたらウィンドウの表示を消す
            checkPhase[2].order.SetActive(false);

            obstruct.SetActive(true);
        }
    }

    // リセットボタンの説明
    void CheckC()
    {
        if (checkPhase[1].order.activeSelf)
        {
            checkPhase[1].order.SetActive(false);
        }
    }

    // リセットボタンを押す
    public void GoToCheckD()
    {
        IsCheckC = true;
    }

    // カメラを初期位置に戻す演出が終わったか
    private bool toCheckD = false;
    public bool ToCheckD
    {
        get { return toCheckD; }
        set
        {
            if(toCheckD || isTutorialComplete) return;
            toCheckD = value;

            // 回転演出が終わったら次へ
            playFunc = CheckD;

            if (!checkPhase[3].order.activeSelf)
            {
                checkPhase[3].order.SetActive(true);
                SEPlay();
            }
        }
    }

    // ダブルタップしたか
    private bool isCheckD = false;
    public bool IsCheckD { 
        get { return isCheckD; } 
        set {
            if (isCheckD || isTutorialComplete) return;
            isCheckD = value;

            // ダブルタップしたらウィンドウを非表示にする
            checkPhase[3].order.SetActive(false);
        } 
    }

    // ダブルタップの説明
    void CheckD()
    {
        if (checkPhase[2].order.activeSelf)
        {
            checkPhase[2].order.SetActive(false);
        }
    }

    // ダブルタップの回転演出が終わったか
    private bool toCheckE = false;
    public bool ToCheckE
    {
        get { return toCheckE; }
        set
        {
            if(toCheckE || isTutorialComplete) return;
            toCheckE = value;

            // 回転演出が終わったら次へ
            playFunc = CheckE;

            if (!checkPhase[4].order.activeSelf)
            {
                checkPhase[4].order.SetActive(true);
                SEPlay();
            }
        }
    }

    // 次のフェーズへの移行ボタンの説明
    void CheckE()
    {
        if (checkPhase[3].order.activeSelf)
        {
            checkPhase[3].order.SetActive(false);
        }

        NextWindowDisplayByTap(checkPhase[4], () => { }, () => 
        {
            ExplainDisplaing(false);
            timeManager.OnStart();
        });
    }

    private bool toSelectA = false;
    public bool ToSelectA
    {
        get
        {
            return toSelectA;
        }
        set
        {
            if(toSelectA || isTutorialComplete) return;
            toSelectA = value;

            ExplainDisplaing(true);
            timeManager.OnStop();

            StartCoroutine(DelayCoroutine(phaseChangeCoolTime, () =>
            {
                playFunc = SelectA;
                obstruct.SetActive(false);
                SEPlay();
            }));
        }
    }

    // 選択フェーズの説明
    void SelectA()
    {
        NextWindowDisplayByTap(selectPhase[0], SelectB, () => 
        {
            StartCoroutine(DelayCoroutine(tapCoolTime, () =>
            {
                if (!selectPhase[1].order.activeSelf)
                {
                    selectPhase[1].order.SetActive(true);
                    SEPlay();
                }
            }));
        });
    }

    // シートの説明
    void SelectB()
    {
        if (selectPhase[0].order.activeSelf)
        {
            selectPhase[0].order.SetActive(false);
        }
    }

    // シートに数字を入力したか
    private bool toSelectC = false;
    public bool ToSelectC
    {
        get
        {
            return toSelectC;
        }
        set
        {
            if (toSelectC || isTutorialComplete) return;
            toSelectC = value;

            // 入力されたら
            StartCoroutine(DelayCoroutine(tapCoolTime, () =>
            {
                selectPhase[1].order.SetActive(false);

                playFunc = SelectC;

                if (!selectPhase[2].order.activeSelf)
                {
                    selectPhase[2].order.SetActive(true);
                    obstruct.SetActive(true);

                    SEPlay();

                    StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                    {
                        toSelectD = true;
                    }));
                }
            }));
        }
    }
    // 消しゴムの説明
    void SelectC()
    {
        if (selectPhase[1].order.activeSelf)
        {
            selectPhase[1].order.SetActive(false);
        }

        if (toSelectD) NextFunctionByTap(selectPhase[2], SelectD, () => 
        {
            if (!selectPhase[3].order.activeSelf)
            {
                selectPhase[3].order.SetActive(true);
                SEPlay();

                StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                {
                    toSelectE = true;
                }));
            }
        });
    }

    private bool toSelectD = false;
    // 虫眼鏡の説明
    void SelectD()
    {
        if (selectPhase[2].order.activeSelf)
        {
            selectPhase[2].order.SetActive(false);
        }

        if (toSelectE) NextFunctionByTap(selectPhase[3], SelectE, () => 
        {
            if (!selectPhase[4].order.activeSelf)
            {
                selectPhase[4].order.SetActive(true);
                SEPlay();

                StartCoroutine(DelayCoroutine(tapCoolTime, () =>
                {
                    toSelectF = true;
                }));
            }
        });
    }

    private bool toSelectE = false;
    // タブの説明
    void SelectE()
    {
        if (selectPhase[3].order.activeSelf)
        {
            selectPhase[3].order.SetActive(false);
        }

        if (toSelectF) NextFunctionByTap(selectPhase[4], SelectF, () => 
        {
            if (!selectPhase[5].order.activeSelf) 
                selectPhase[5].order.SetActive(true);
            SEPlay();
        });
    }

    private bool toSelectF = false;
    // 次のフェーズへの移行ボタンの説明
    void SelectF()
    {
        if (selectPhase[4].order.activeSelf)
        {
            selectPhase[4].order.SetActive(false);
        }

        NextWindowDisplayByTap(selectPhase[5], () => { }, () => 
        {
            ExplainDisplaing(false);
            timeManager.OnStart();
        });
    }

    /// <summary>
    /// 実行フェーズの説明を始める
    /// </summary>
    public void ToPlayA()
    {
        StartCoroutine(DelayCoroutine(phaseChangeCoolTime, () =>
        {
            playFunc = PlayA;
            ExplainDisplaing(true);
            timeManager.OnStop();

            SEPlay();
        }));
    }

    // 実行フェーズの説明
    void PlayA()
    {
        NextWindowDisplayByTap(playPhase[0], () => { }, () =>
        {
            endPlayA = true;
        });
    }

    // 実行フェーズの説明が終わったか
    private bool endPlayA = false;
    public bool EndPlayA { get { return endPlayA; } private set { } }

    // 落下演出が終わったか
    private bool toPlayB = false;
    public bool ToPlayB
    {
        get
        {
            return toPlayB;
        }
        set
        {
            if (toPlayB || isTutorialComplete) return;
            toPlayB = value;

            playFunc = PlayB;

            SEPlay();
        }
    }

    // クリア条件の説明
    void PlayB()
    {
        if (playPhase[0].order.activeSelf)
        {
            playPhase[0].order.SetActive(false);
        }

        NextWindowDisplayByTap(playPhase[1], () => { }, () => 
        {
            endPlayB = true;
        });
    }

    // クリア条件の説明が終わったか
    private bool endPlayB = false;
    public bool EndPlayB { get { return endPlayB; } private set { } }

    // リザルト表示しているか
    private bool toPlayC = false;
    public bool ToPlayC
    {
        get
        {
            return toPlayC;
        }
        set
        {
            if (toPlayC || isTutorialComplete) return;
            toPlayC = value;

            playFunc = PlayC;
            SEPlay();
        }
    }

    private bool endPlayC = false;
    public bool EndPlayC { get { return endPlayC;} }

    // ミッションの説明、チュートリアルの終了
    void PlayC()
    {
        if (playPhase[1].order.activeSelf)
        {
            playPhase[1].order.SetActive(false);
        }

        NextWindowDisplayByTap(playPhase[2], () => { }, () => 
        {
            ExplainDisplaing(false);
            endPlayC = true;
            isTutorialComplete = true;
            obstruct.SetActive(false);
        });
    }

    bool isFirstChange = true;

    /// <summary>
    /// タップで次の関数へ移行する
    /// </summary>
    /// <param name="_unDispWindow">非表示にするウィンドウ</param>
    /// <param name="_nextFunc">次の関数</param>
    void NextFunctionByTap(Windows _unDispWindow, PlayFunc _nextFunc, Action _lastFunc)
    {
        if (isFirstChange)
        {
            StartCoroutine(CoolTime());
            isFirstChange = false;
        }

        StartCoroutine(CoolTimeEnd(() => {
            if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                if (_unDispWindow.order.activeSelf)
                {
                    _unDispWindow.order.SetActive(false);
                }

                playFunc = _nextFunc;
                _lastFunc?.Invoke();
                isFirstChange = true;

                StartCoroutine(CoolTime());
            }
        }));
    }

    /// <summary>
    /// タップで表示ウィンドウを順番に変更する
    /// </summary>
    /// <param name="_windows">対象ウィンドウ</param>
    /// <param name="_nextFunc">次に呼ぶ関数</param>
    /// <param name="_lastFunc">ウィンドウを閉じるときに行う処理</param>
    void NextWindowDisplayByTap(Windows _windows, PlayFunc _nextFunc, Action _lastFunc)
    {
        if (isFirstChange)
        {
            // ウィンドウの親を表示
            if (!_windows.order.activeSelf) _windows.order.SetActive(true);

            StartCoroutine(CoolTime());
            isFirstChange = false;
        }

        StartCoroutine(CoolTimeEnd(() => {
            // タップで次の説明へ
            if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
            {
                tapCount++;

                for (int i = 0; i < _windows.objects.Length; i++)
                {
                    if (i == tapCount)
                    {
                        SEPlay();
                        _windows.objects[i].SetActive(true);
                    }
                    else _windows.objects[i].SetActive(false);
                }

                if (tapCount >= _windows.objects.Length)
                {
                    _windows.order.SetActive(false);
                    tapCount = 0;

                    StartCoroutine(CoolTime());
                    StartCoroutine(CoolTimeEnd(() =>
                    {
                        playFunc = _nextFunc;

                        _lastFunc?.Invoke();
                        isFirstChange = true;
                    }));
                    return;
                }

                StartCoroutine(CoolTime());
            }
        }));
    }

    /// <summary>
    /// 説明ウィンドウ表示中かどうかを設定
    /// </summary>
    /// <param name="_isDisplaing">trueのとき、説明中　操作に制限を掛ける + 設定ボタン使用不可</param>
    void ExplainDisplaing(bool _isDisplaing)
    {
        // 自由に操作できるかを設定
        tutorialCompleteByPhase = !_isDisplaing;

        // 設定ボタンを押せるかを設定
        obstruct.SetActive(_isDisplaing);
    }

    void SEPlay()
    {
        if (soundManager != null && !isTutorialComplete)
        {
            soundManager.SEPlay7();
        }
    }

    IEnumerator CoolTime()
    {
        isCoolTime = true;
        yield return coolTime;
        isCoolTime = false;
    }

    IEnumerator CoolTimeEnd(Action _action)
    {
        yield return new WaitUntil(() => !isCoolTime);
        _action?.Invoke();
    }
}