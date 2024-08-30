using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System;
using static Extensions;

/// <summary>
/// 実行フェーズ
/// </summary>
public class PlayPhase : MonoBehaviour, IPhase
{
    [SerializeField] private StageController stageController;
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private MeshCombiner meshCombiner;
    [SerializeField] private Vibration vibration;

    [SerializeField] private Transform objParent; // 親オブジェクト
    [SerializeField] private Transform clearObjParent;

    [SerializeField] private GameObject playPhaseUI;

    private string stageName;
    private Vector3 mapSize = new Vector3(0, 0, 0);

    private ShapeData.Shape[,,] map; // 配置データ
    private GameObject[,,] mapObj;   // GameObject型配列

    private ShapeData.Shape[,,] correctAnswer; // 正答
    private ShapeData.Shape[,,] lastAnswer;    // 前の回答

    private bool isFalling = false;

    // 落下スキップ
    private bool isSkip = false;

    // 早送り
    public bool IsFastForward { get; private set; } = false;
    [field: SerializeField, Header("早送りの倍率")] public float FastForwardRatio { get; private set; }

    [SerializeField, Header("落下時の振動の長さ(秒) 通常")] private float fallVibrateTime_Normal;
    [SerializeField, Header("落下時の振動の長さ(秒) 早送り")] private float fallVibrateTime_FastForward;
    [SerializeField, Header("クリア時の振動の長さ(秒)")] private float clearVibrateTime;

    [SerializeField, Header("落下速度")] private float fallSpeed;
    [SerializeField, Header("オブジェクトを落とす高さ")] private int fallPos;
    [SerializeField, Header("オブジェクトを落とす間隔(sec)")] private float fallInterval;
    [SerializeField, Tooltip("オブジェクトが全て落下してから一致率表示までの時間(sec)")]
    private float fallToMatchdispTime;

    // 一致率計算
    private float matchRateTroutTotal = 0; // 1マスあたりの一致率の和
    private int hasBlockTrout = 0; // 1つ以上ブロックがあるマスの数
    private int matchRate = 0; // 一致率

    [SerializeField] private Text matchRateText;
    [SerializeField] private GameObject matchUI;
    [SerializeField] private GameObject completeText;
    [SerializeField] private Animator completeAnim;
    [SerializeField] private ParticleSystem completeEffect;

    private bool toClearWindow = false;
    [SerializeField] private ResultWindow resultWindow;
    [SerializeField] private MissionWindow missionWindow;
    [SerializeField] private MissionScore missionScore;

    public void Initialize()
    {
        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage;
        else stageName = stageController.StageName;

        playPhaseUI.SetActive(false);
        resultWindow.gameObject.SetActive(false);
        missionWindow.gameObject.SetActive(false);
        matchUI.SetActive(false);
        matchRateText.enabled = false;

        toClearWindow = false;

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列 要素数指定
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        lastAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = ShapeData.Shape.Empty;
                    mapObj[x, y, z] = null;
                    correctAnswer[x, y, z] = ShapeData.Shape.Empty;
                    lastAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
    }

    /// <summary>
    /// 実行フェーズ移行時の処理
    /// </summary>
    public void PhaseStart()
    {
        playPhaseUI.SetActive(true);

        // 正答とプレイヤーの解答を取得する
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    correctAnswer[x, y, z] = stageController.CorrectAnswer[x, y, z];
                    lastAnswer[x, y, z] = map[x, y, z];
                    map[x, y, z] = stageController.PlayerAnswer[x, y, z];
                }
            }
        }

        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;
        isSkip = false;
        IsFastForward = false;

        AnswerInstance();
    }

    public void PhaseUpdate()
    {
        if (stageController.phase != StageController.PHASE.PLAY) return;

        if (isFalling)
        {
            Skip();
            FastForward();
        }

        MeshCombine();
        ClearCheck();
    }

    /// <summary>
    /// 実行フェーズ終了
    /// </summary>
    public void PhaseEnd()
    {
        StopAllCoroutines();

        matchRateText.enabled = false;
        matchUI.SetActive(false);
        playPhaseUI.SetActive(false);
        resultWindow.gameObject.SetActive(false);
        missionWindow.gameObject.SetActive(false);

        tapManager.LongTapReset();
        tapManager.DoubleTapReset();

        // ブロック非表示
        Transform children = objParent.GetComponentInChildren<Transform>();
        foreach (Transform obj in children)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }

        children = clearObjParent.GetComponentInChildren<Transform>();
        foreach (Transform obj in children)
        {
            obj.GetComponent<MeshRenderer>().enabled = false;
        }

        RemoveMeshes();
    }

    /// <summary>
    /// 落下演出スキップ
    /// </summary>
    void Skip()
    {
        tapManager.DoubleTap(() =>
        {
            if (isFalling)
            {
                isSkip = true;
                IsFastForward = false;
            }
        });
    }

    /// <summary>
    /// 落下演出早送り
    /// </summary>
    void FastForward()
    {
        tapManager.LongTap(() =>
            {
                IsFastForward = true;
                isSkip = false;
            }, () =>
            {
                IsFastForward = false;
            }, 0.5f);
    }

    void MeshCombine()
    {
        meshCombiner.SetParent(clearObjParent);
        meshCombiner.Combine(true);
    }

    void RemoveMeshes()
    {
        meshCombiner.Remove();
    }

    /// <summary>
    /// 解答のモデルを生成、一致率の計算
    /// </summary>
    void AnswerInstance()
    {
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                float matchRateTrout = 0; // 1マス内の一致率

                float total = 0; // 1マス内のブロックの総数
                float c_total = 0; // 正解の1マス内のブロックの総数
                float excess = 0; // 超過数
                float lack = 0; // 不足数
                float diff = 0; // 相違数

                bool hasAnything = false; // 正答/プレイヤーの解答のどちらかで、何か置かれているマスならtrue

                for (int y = 0; y < mapSize.y; y++)
                {
                    // 生成
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    // 前の解答と異なっていたら生成し直す
                    if (map[x, y, z] != lastAnswer[x, y, z])
                    {
                        if (mapObj[x, y, z]) Destroy(mapObj[x, y, z]);

                        // 空白部分は生成しない
                        if (map[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Alpha) 
                            mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

                        // 半透明ブロックは別オブジェクトの子として生成
                        if (map[x, y, z] == ShapeData.Shape.Alpha)
                            mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, clearObjParent);
                    }
                    else
                    {
                        if (mapObj[x, y, z])
                        {
                            mapObj[x, y, z].transform.position = pos;
                            mapObj[x, y, z].GetComponent<MeshRenderer>().enabled = true;
                        }
                    }

                    // 正答とプレイヤーの解答を比べる
                    if (map[x, y, z] != ShapeData.Shape.Empty)
                        total++;
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty)
                        c_total++;

                    // 超過分 正答では何も置かれていない場所にオブジェクトが置かれた場合
                    if (correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                        excess++;

                    // 不足分 正答では何か置かれている場所にオブジェクトが何も置かれていなかった場合
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] == ShapeData.Shape.Empty)
                        lack++;

                    // 相違分 形が違う場合 かつ 空でない場合
                    if (correctAnswer[x, y, z] != map[x, y, z] && correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                        diff++;

                    // 一致率計算の分母　正答/プレイヤーの解答のどちらかで何か置かれているマスだったら
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty || map[x, y, z] != ShapeData.Shape.Empty)
                        hasAnything = true;
                }

                if (hasAnything) hasBlockTrout++;

                // 1マスあたりの一致率を計算
                if (excess > 0) // 超過がある場合
                {
                    matchRateTrout = (float)(1 - ((total - (c_total - diff)) / total));
                    // 1 - (( 1マス内の総ブロック数 ) - (( 正答の1マス内のブロック数 ) - 相違数 ) / 1マス内の総ブロック数 )
                }
                else
                {
                    if (c_total > 0)
                        matchRateTrout = ((c_total - lack - diff) / c_total);
                    // (( 正答の1マス内のブロック数 ) - ( 不足分 ) - ( 相違分 )) / ( 正答の1マス内のブロック数 )

                    // 何も置いてない場合は0
                    if (c_total <= 0)
                        matchRateTrout = 0;
                }

                matchRateTroutTotal += matchRateTrout;
            }
        }

        StartCoroutine(Fall());
    }

    /// <summary>
    /// 落下
    /// </summary>
    IEnumerator Fall()
    {
        if(stageController.IsTutorial)
        {
            yield return new WaitUntil (() => tutorial.EndPlayA);
        }

        isFalling = true;

        GameObject finalObj = mapObj[0, 0, 0]; // 最後のオブジェクト

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    if (mapObj[x, y, z])
                    {
                        ShapeObjects shapeObj = mapObj[x, y, z].GetComponent<ShapeObjects>();
                        shapeObj.TargetHeight = y;
                        shapeObj.FallSpeed = fallSpeed;
                        shapeObj.IsFall = true;
                        shapeObj.IsVibrate = true;

                        finalObj = mapObj[x, y, z];
                    }

                    if (!isSkip)
                        yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        if(finalObj)
        {
            var so = finalObj.GetComponent<ShapeObjects>();

            if(so != null)
            {
                // 最後のオブジェクトが落ちたら
                yield return new WaitUntil(() => !so.IsFall);
            }
        }

        isFalling = false;
        
        if (stageController.IsTutorial)
        {
            yield return new WaitUntil(() => tutorial.EndPlayA);
            ResultDisp();
        }
        else Invoke("ResultDisp", fallToMatchdispTime);
    }

    /// <summary>
    /// リザルト表示
    /// </summary>
    void ResultDisp()
    {
        // 全体の一致率算出
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);
        matchRateText.text = matchRate.ToString() + "%";
        matchUI.SetActive(true);

        float waitTime = 0.0f;
        if (stageController.IsTutorial) waitTime = 0.5f;

        StartCoroutine(DelayCoroutine(waitTime, () =>
        {
            if (matchRate >= 100)
            {
                if(stageController.IsTutorial) tutorial.ToPlayB = true;

                completeText.SetActive(true);
                completeAnim.SetTrigger("Completed");
                completeEffect.Play();

                // 100％演出が終了したら
                StartCoroutine(DelayCoroutine(() =>
                {
                    if (completeAnim.GetCurrentAnimatorStateInfo(0).IsName("End")) return true;
                    else return false;
                }, () =>
                {
                    // Tutorialの場合は説明終了まで待つ
                    StartCoroutine(DelayCoroutine(() =>
                    {
                        if (stageController.IsTutorial) return tutorial.EndPlayB;
                        else return true;
                    }, () =>
                    {
                        // リザルト表示
                        resultWindow.gameObject.SetActive(true);
                        missionScore.ResultDisp();

                        vibration.PluralVibrate(2, (long)(clearVibrateTime * 1000));

                        toClearWindow = true;
                    }));
                }));
            }
            else
            {
                completeText.SetActive(false);
                matchRateText.enabled = true;
                StartCoroutine(MatchTextBlinking());
            }
        }));
    }

    const float UN_DISP_TIME = 0.3f;
    const float DISP_TIME = 0.5f;

    /// <summary>
    /// パーセンテージ点滅演出
    /// </summary>
    IEnumerator MatchTextBlinking()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(DISP_TIME);
            matchRateText.enabled = false;
            yield return new WaitForSeconds(UN_DISP_TIME);
            matchRateText.enabled = true;
        }

        yield return new WaitForSeconds(DISP_TIME);

        if (stageController.IsTutorial)
        {
            tutorial.ToPlayB = true;
            yield return new WaitUntil(() => tutorial.EndPlayB);
            yield return new WaitForSeconds(0.5f);
            tutorial.ToPlayC = true;
            yield return new WaitUntil(() => tutorial.EndPlayC);
        }

        stageController.IsRetry = true;
    }

    void ClearCheck()
    {
        // リザルト表示完了後にタップしたら
        if (resultWindow.DispEnd && toClearWindow && Input.touchCount >= 1)
        {
            // 通常ステージかチュートリアルステージの場合ミッション達成画面を表示
            if (stageName.Contains("Stage") || stageName == "Tutorial")
            {
                resultWindow.gameObject.SetActive(false);
                missionWindow.gameObject.SetActive(true);

                // 表示終了後
                if (missionWindow.DispEnd)
                {
                    // チュートリアルの場合
                    if (stageController.IsTutorial)
                    {
                        // 0.5秒待って説明ウィンドウ表示
                        // 全ウィンドウの表示が完了したら0.5秒待って遷移
                        StartCoroutine(DelayCoroutine(0.5f, () =>
                        {
                            tutorial.ToPlayC = true;

                            StartCoroutine(DelayCoroutine(() =>
                            {
                                return tutorial.EndPlayC;
                            }, () =>
                            {
                                StartCoroutine(DelayCoroutine(0.5f, () =>
                                {
                                    stageController.IsClear = true;
                                }));

                            }));
                        }));
                    }

                    else
                    {
                        // 0.2秒待って遷移
                        StartCoroutine(DelayCoroutine(0.2f, () =>
                        {
                            stageController.IsClear = true;
                        }));
                    }
                }
            }
            else
            {
                // 0.2秒待って遷移
                StartCoroutine(DelayCoroutine(0.2f, () =>
                {
                    stageController.IsClear = true;
                }));
            }
        }
    }

    /// <summary>
    /// 振動時間取得
    /// </summary>
    /// <returns>[0]が通常、[1]が早送り時の振動の長さ</returns>
    public float[] GetVibrateTime()
    {
        float[] t = { fallVibrateTime_Normal, fallVibrateTime_FastForward };
        return t;
    }
}