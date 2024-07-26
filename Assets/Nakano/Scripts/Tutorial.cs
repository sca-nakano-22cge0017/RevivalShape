using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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

    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Windows[] checkPhase;
    [SerializeField] private Windows[] selectPhase;
    [SerializeField] private Windows[] playPhase;

    [SerializeField, Header("UI操作不可にする用Image")] GameObject obstruct;

    private int tapCount = 0;
    private bool tutorialCompleteByPhase = false;
    /// <summary>
    /// 各フェーズのチュートリアルが終了しているか
    /// </summary>
    public bool TutorialCompleteByPhase { get { return tutorialCompleteByPhase; } private set { tutorialCompleteByPhase = value; } }

    delegate void PlayFunc();
    PlayFunc playFunc;
    private bool isFirstSelectPhase = true;
    private bool isFirstPlayPhase = true;

    private bool isTutorialComplete = false;
    public bool IsTutorialComplete { get { return isTutorialComplete; } private set { } }

    private string methodName = "";
    public string MethodName { get { return methodName; } private set { methodName = value; } }

    [SerializeField, Header("タップしてから次にタップできるようになるまでの時間")] float tapCoolTime = 0.5f;

    public void TutorialStart()
    {
        tutorialCanvas.SetActive(true);
        obstruct.SetActive(true);
        playFunc = CheckA;
    }

    public void TutorialUpdate()
    {
        if (playFunc != null && !isTutorialComplete)
        {
            playFunc();
            methodName = playFunc.Method.Name;
        }
    }

    // 確認フェーズの説明
    void CheckA()
    {
        NextWindowDisplayByTap(checkPhase[0], CheckB, () =>{ });
    }

    // 右方向へスワイプしたか
    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { 
        get { return toCheckB_2;} 
        set { 
            toCheckB_2 = true;

            // 左スワイプ指示
            if (!checkPhase[1].objects[1].activeSelf)
            {
                checkPhase[1].objects[0].SetActive(false);
                checkPhase[1].objects[1].SetActive(true);
            }
        } 
    }

    // 左方向へスワイプしたか
    private bool toCheckC = false;
    public bool ToCheckC { 
        get { return toCheckC; } 
        set { 
            toCheckC = true;

            // 次の処理へ
            checkPhase[1].order.SetActive(false);
            checkPhase[1].objects[1].SetActive(false);
            playFunc = CheckC;
        }
    }
    // ドラッグ操作の説明
    void CheckB()
    {
        // 右移動指示
        if (!checkPhase[1].order.activeSelf)
        {
            checkPhase[1].order.SetActive(true);
            checkPhase[1].objects[0].SetActive(true);
        }
    }

    // リセットボタンを押したか
    private bool isCheckC = false;
    public bool IsCheckC { 
        get { return isCheckC; } 
        set { 
            isCheckC = value;

            // リセットボタンが押されたらウィンドウの表示を消す
            checkPhase[2].order.SetActive(false);
        }
    }

    // カメラを初期位置に戻す演出が終わったか
    private bool toCheckD = false;
    public bool ToCheckD { 
        get { return toCheckD; } 
        set { 
            toCheckD = value;

            // 回転演出が終わったら次へ
            playFunc = CheckD;
        } 
    }

    // リセットボタンを押す
    public void GoToCheckD()
    {
        IsCheckC = true;
    }

    // リセットボタンの説明
    void CheckC()
    {
        if (!checkPhase[2].order.activeSelf && !isCheckC)
        {
            checkPhase[2].order.SetActive(true);
        }
    }

    // ダブルタップしたか
    private bool isCheckD = false;
    public bool IsCheckD { 
        get { return isCheckD; } 
        set { 
            isCheckD = value;

            // ダブルタップしたらウィンドウを非表示にする
            checkPhase[3].order.SetActive(false);
        } 
    }

    // ダブルタップの回転演出が終わったか
    private bool toCheckE = false;
    public bool ToCheckE { 
        get { return toCheckE; } 
        set { 
            toCheckE = value;

            // 回転演出が終わったら次へ
            playFunc = CheckE;
        }
    }

    // ダブルタップの説明
    void CheckD()
    {
        if (!checkPhase[3].order.activeSelf && !isCheckD)
        {
            checkPhase[3].order.SetActive(true);
        }
    }

    // 次のフェーズへの移行ボタンの説明
    void CheckE()
    {
        NextWindowDisplayByTap(checkPhase[4], () => { }, () => 
        {
            ExplainDisplaing(false);
        });
    }

    /// <summary>
    /// 選択フェーズの説明を始める
    /// </summary>
    public void ToSelectA()
    {
        playFunc = SelectA;
        ExplainDisplaing(true);
    }

    // 選択フェーズの説明
    void SelectA()
    {
        if(!isFirstSelectPhase) return;

        NextWindowDisplayByTap(selectPhase[0], SelectB, () => { });
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
            if(toSelectC) return;

            toSelectC = value;

            // 入力されたら
            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () => 
            {
                playFunc = SelectC;
                selectPhase[1].order.SetActive(false);
            }));
        }
    }

    // シートの説明
    void SelectB()
    {
        if (!selectPhase[1].order.activeSelf)
        {
            selectPhase[1].order.SetActive(true);
        }
    }

    private bool toSelectD = false;

    // 消しゴムの説明
    void SelectC()
    {
        if (!selectPhase[2].order.activeSelf)
        {
            selectPhase[2].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectD = true;
            }));
        }

        if(toSelectD) NextFunctionByTap(selectPhase[2], SelectD);
    }

    private bool toSelectE = false;

    // 虫眼鏡の説明
    void SelectD()
    {
        if (!selectPhase[3].order.activeSelf)
        {
            selectPhase[3].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectE = true;
            }));
        }

        if (toSelectE) NextFunctionByTap(selectPhase[3], SelectE);
    }

    private bool toSelectF = false;

    // タブの説明
    void SelectE()
    {
        if (!selectPhase[4].order.activeSelf)
        {
            selectPhase[4].order.SetActive(true);

            StartCoroutine(stageController.DelayCoroutine(tapCoolTime, () =>
            {
                toSelectF = true;
            }));
        }

        if (toSelectF) NextFunctionByTap(selectPhase[4], SelectF);
    }

    // 次のフェーズへの移行ボタンの説明
    void SelectF()
    {
        NextWindowDisplayByTap(selectPhase[5], () => { }, () => 
        {
            ExplainDisplaing(false);
            isFirstSelectPhase = false;
        });
    }

    /// <summary>
    /// 実行フェーズの説明を始める
    /// </summary>
    public void ToPlayA()
    {
        playFunc = PlayA;
        ExplainDisplaing(true);
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
            if(toPlayB) return;
            toPlayB = value;

            playFunc = PlayB;
        }
    }

    // 実行フェーズの説明
    void PlayA()
    {
        if (!isFirstPlayPhase) return;

        NextWindowDisplayByTap(playPhase[0], () => { }, () =>
        {
            endPlayA = true;
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

    // ミッションの説明、チュートリアルの終了
    void PlayC()
    {
        NextWindowDisplayByTap(playPhase[2], () => { }, () => 
        {
            ExplainDisplaing(false);
            isFirstPlayPhase = false;
            isTutorialComplete = true;
        });
    }

    /// <summary>
    /// タップで次の関数へ移行する
    /// </summary>
    /// <param name="_unDispWindow">非表示にするウィンドウ</param>
    /// <param name="_nextFunc">次の関数</param>
    void NextFunctionByTap(Windows _unDispWindow, PlayFunc _nextFunc)
    {
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            playFunc = _nextFunc;

            if (_unDispWindow.order.activeSelf)
            {
                _unDispWindow.order.SetActive(false);
            }
        }
    }

    /// <summary>
    /// タップで表示ウィンドウを順番に変更する
    /// </summary>
    /// <param name="_windows">対象ウィンドウ</param>
    /// <param name="_nextFunc">次に呼ぶ関数</param>
    /// <param name="_lastFunc">処理終了時に行う特殊処理</param>
    void NextWindowDisplayByTap(Windows _windows, PlayFunc _nextFunc, Action _lastFunc)
    {
        if(!_windows.order.activeSelf) _windows.order.SetActive(true);

        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapCount++;

            for (int i = 0; i < _windows.objects.Length; i++)
            {
                if (i == tapCount)
                {
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
        tutorialCompleteByPhase = !_isDisplaing;
        obstruct.SetActive(_isDisplaing);
    }
}