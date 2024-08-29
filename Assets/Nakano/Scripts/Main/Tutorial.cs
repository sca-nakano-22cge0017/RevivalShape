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
        playFunc = CheckA;

        timeManager.OnStop();

        SEPlay();
    }

    public void TutorialUpdate()
    {
        if (playFunc != null && !isTutorialComplete)
        {
            playFunc();
            methodName = playFunc.Method.Name;
        }

        if(isTutorialComplete)
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
        });
    }

    // 右方向へスワイプしたか
    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { 
        get { return toCheckB_2;} 
        set {
            if(toCheckB_2) return;
            toCheckB_2 = value;

            // 左スワイプ指示
            if (!checkPhase[1].objects[1].activeSelf)
            {
                checkPhase[1].objects[0].SetActive(false);
                checkPhase[1].objects[1].SetActive(true);
            }
        } 
    }
    // ドラッグ操作の説明
    void CheckB()
    {
    }

    // 左方向へスワイプしたか
    private bool toCheckC = false;
    public bool ToCheckC
    {
        get { return toCheckC; }
        set
        {
            if (toCheckC) return;
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
            if (isCheckC) return;
            isCheckC = value;

            // リセットボタンが押されたらウィンドウの表示を消す
            checkPhase[2].order.SetActive(false);

            obstruct.SetActive(true);
        }
    }

    // リセットボタンの説明
    void CheckC()
    {
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
            if(toCheckD) return;
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
            if (isCheckD) return;
            isCheckD = value;

            // ダブルタップしたらウィンドウを非表示にする
            checkPhase[3].order.SetActive(false);
        } 
    }

    // ダブルタップの説明
    void CheckD()
    {
    }

    // ダブルタップの回転演出が終わったか
    private bool toCheckE = false;
    public bool ToCheckE
    {
        get { return toCheckE; }
        set
        {
            if(toCheckE) return;
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
            if(toSelectA) return;
            toSelectA = value;

            playFunc = SelectA;
            ExplainDisplaing(true);

            obstruct.SetActive(false);
            timeManager.OnStop();

            SEPlay();
        }
    }

    // 選択フェーズの説明
    void SelectA()
    {
        NextWindowDisplayByTap(selectPhase[0], SelectB, () => 
        {
            if (!selectPhase[1].order.activeSelf)
            {
                selectPhase[1].order.SetActive(true);
                SEPlay();
            }
        });
    }

    // シートの説明
    void SelectB()
    {
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
            if (toSelectC) return;
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
        if(toSelectD) NextFunctionByTap(selectPhase[2], SelectD, () => 
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
        if (toSelectF) NextFunctionByTap(selectPhase[4], SelectF, () => 
        {
            if (!selectPhase[5].order.activeSelf) selectPhase[5].order.SetActive(true);
            SEPlay();
        });
    }

    private bool toSelectF = false;
    // 次のフェーズへの移行ボタンの説明
    void SelectF()
    {
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
        playFunc = PlayA;
        ExplainDisplaing(true);
        timeManager.OnStop();

        SEPlay();
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
            if (toPlayB) return;
            toPlayB = value;

            playFunc = PlayB;

            SEPlay();
        }
    }

    // クリア条件の説明
    void PlayB()
    {
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
            if (toPlayC) return;
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
        NextWindowDisplayByTap(playPhase[2], () => { }, () => 
        {
            ExplainDisplaing(false);
            endPlayC = true;
            isTutorialComplete = true;
            timeManager.OnStart();
        });
    }

    /// <summary>
    /// タップで次の関数へ移行する
    /// </summary>
    /// <param name="_unDispWindow">非表示にするウィンドウ</param>
    /// <param name="_nextFunc">次の関数</param>
    void NextFunctionByTap(Windows _unDispWindow, PlayFunc _nextFunc, Action _lastFunc)
    {
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (_unDispWindow.order.activeSelf)
            {
                _unDispWindow.order.SetActive(false);
            }

            playFunc = _nextFunc;
            _lastFunc?.Invoke();
        }
    }

    /// <summary>
    /// タップで表示ウィンドウを順番に変更する
    /// </summary>
    /// <param name="_windows">対象ウィンドウ</param>
    /// <param name="_nextFunc">次に呼ぶ関数</param>
    /// <param name="_lastFunc">ウィンドウを閉じるときに行う処理</param>
    void NextWindowDisplayByTap(Windows _windows, PlayFunc _nextFunc, Action _lastFunc)
    {
        // ウィンドウの親を表示
        if (!_windows.order.activeSelf) _windows.order.SetActive(true);

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

                playFunc = _nextFunc;

                _lastFunc?.Invoke();
            }
        }
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
        if (soundManager != null)
        {
            soundManager.SEPlay7();
        }
    }
}