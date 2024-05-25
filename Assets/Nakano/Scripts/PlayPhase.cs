using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 実行フェーズ
/// </summary>
public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;
    [SerializeField] MissionCheck missionCheck;
    Vibration vibration;

    // 親オブジェクト
    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map; // 配置データ
    GameObject[,,] mapObj;   // GameObject型配列

    ShapeData.Shape[,,] correctAnswer; // 正答

    bool isFalling = false;

    // 落下スキップ
    bool isSkip = false;
    int skipTapCount = 0;
    float skipTime = 0f;
    bool canJudgement = false;

    // 早送り
    public bool IsFastForward { get; private set; } = false;
    float longTapTime = 0;
    bool countStart = false;
    [field: SerializeField, Header("早送りの倍率")] public float FastForwardRatio { get; private set; }

    [SerializeField, Header("落下時の振動の長さ(秒)")] float fallVibrateTime;
    [SerializeField, Header("クリア時の振動の長さ(秒)")] float clearVibrateTime;

    [SerializeField, Header("落下速度")] float fallSpeed;
    [SerializeField, Header("オブジェクトを落とす高さ")] int fallPos;
    [SerializeField, Header("オブジェクトを落とす間隔(sec)")] float fallInterval;
    [SerializeField, Tooltip("オブジェクトが全て落下してから一致率表示までの時間(sec)")]
    float fallToMatchdispTime;

    // 一致率計算
    float matchRateTroutTotal = 0; // 1マスあたりの一致率の和
    int hasBlockTrout = 0; // 1つ以上ブロックがあるマスの数
    int matchRate = 0; // 一致率

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;

    /// <summary>
    /// 確認中か
    /// </summary>
    public bool IsDebug { get; private set; } = false;

    private void Awake()
    {
        // UI等を消しておく
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;

        vibration = GameObject.FindObjectOfType<Vibration>();
    }

    private void Update()
    {
        Skip();
        FastForward();
    }

    /// <summary>
    /// 落下演出スキップ
    /// </summary>
    void Skip()
    {
        if (!isFalling) return;

        // 1回目のタップ
        if (Input.GetMouseButtonDown(0) && skipTapCount == 0)
        {
            // 範囲外は無効
            if (stageController.TapOrDragRange(Input.mousePosition)) return;

            skipTapCount++;
            canJudgement = true;
        }

        if (canJudgement) skipTime += Time.deltaTime;

        if (skipTime <= 0.5f && skipTime >= 0.01f)
        {
            // 2回目のタップ
            if (Input.GetMouseButtonDown(0))
            {
                // 範囲外は無効
                if (stageController.TapOrDragRange(Input.mousePosition)) return;

                skipTapCount++;
            }
        }
        else if (skipTime > 0.5f)
        {
            canJudgement = false;
            skipTapCount = 0;
            skipTime = 0f;
        }

        if (skipTapCount >= 2)
        {
            isSkip = true;
            canJudgement = false;
            skipTime = 0f;
        }
        else isSkip = false;
    }

    /// <summary>
    /// 落下演出早送り
    /// </summary>
    void FastForward()
    {
        if (!isFalling) return;

        if (Input.GetMouseButtonDown(0) && !IsFastForward)
        {
            // 範囲外は無効
            if(stageController.TapOrDragRange(Input.mousePosition)) return;

            countStart = true;
            longTapTime = 0;
        }

        if (countStart)
        {
            longTapTime += Time.deltaTime;
        }

        if (longTapTime >= 0.5f)
        {
            IsFastForward = true;
        }

        if (Input.GetMouseButtonUp(0) && (IsFastForward || countStart))
        {
            // 範囲外は無効
            if (stageController.TapOrDragRange(Input.mousePosition)) return;

            longTapTime = 0;
            IsFastForward = false;
            countStart = false;
        }
    }

    /// <summary>
    /// 実行フェーズ移行時の処理
    /// </summary>
    public void PlayPhaseStart()
    {
        playPhaseUI.SetActive(true);

        // サイズ代入
        mapSize = stageController.MapSize;

        // 配列 要素数指定
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // 正答とプレイヤーの解答を取得する
        correctAnswer = stageController.CorrectAnswer;
        map = stageController.PlayerAnswer;

        // 一致率 初期化
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;

        AnswerInstance();

        isSkip = false;
        skipTapCount = 0;
        skipTime = 0f;
        canJudgement = false;

        IsFastForward = false;
        longTapTime = 0;
        countStart = false;
    }

    /// <summary>
    /// 実行フェーズ終了
    /// </summary>
    public void PlayPhaseEnd()
    {
        // ブロック削除
        Transform children = objParent.GetComponentInChildren<Transform>();
        foreach (Transform obj in children)
        {
            Destroy(obj.gameObject);
        }

        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);
        matchRateText.enabled = false;
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

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

                    // 正答とプレイヤーの解答を比べる
                    if (map[x, y, z] != ShapeData.Shape.Empty) total++;
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty) c_total++;

                    // 超過分 正答では何も置かれていない場所にオブジェクトが置かれた場合
                    if (correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        excess++;
                    }

                    // 不足分 正答では何か置かれている場所にオブジェクトが何も置かれていなかった場合
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] == ShapeData.Shape.Empty)
                    {
                        lack++;
                    }

                    // 相違分 形が違う場合 かつ 空でない場合
                    if (correctAnswer[x, y, z] != map[x, y, z] && correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        diff++;
                    }

                    // 一致率計算の分母　正答/プレイヤーの解答のどちらかで何か置かれているマスだったら
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty || map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        hasAnything = true;
                    }
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
        isFalling = true;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    // 空白マスなら落下演出を飛ばす
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    mapObj[x, y, z].GetComponent<ShapeObjects>().TargetHeight = y;
                    mapObj[x, y, z].GetComponent<ShapeObjects>().FallSpeed = fallSpeed;
                    mapObj[x, y, z].GetComponent<ShapeObjects>().IsFall = true;

                    StartCoroutine(VibrateOn(mapObj[x, y, z]));

                    if (!isSkip)
                        yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        isFalling = false;
        isSkip = false;
        IsFastForward = false;

        yield return new WaitForSeconds(fallToMatchdispTime);
        MatchRateDisp();
    }

    /// <summary>
    /// 振動オン
    /// </summary>
    /// <param name="obj">振動をオンにしたいオブジェクト</param>
    /// <returns></returns>
    IEnumerator VibrateOn(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        obj.GetComponent<ShapeObjects>().VibrateTime = fallVibrateTime;
        obj.GetComponent<ShapeObjects>().IsVibrate = true; // 振動オン
    }

    /// <summary>
    /// プレイヤーの解答と正答の一致率を確認・表示
    /// </summary>
    void MatchRateDisp()
    {
        // 一致率算出
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);

        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        // 100%でクリア
        if (matchRate >= 100)
        {
            clearWindow.SetActive(true);
            missionCheck.Mission();

            vibration.PluralVibrate(2, (long)(clearVibrateTime * 1000));

            stageController.IsClear = true;
        }
        else
        {
            StartCoroutine(MatchTextBlinking());

            stageController.IsRetry = true;
        }
    }

    float unDispTime = 0.3f;
    float dispTime = 0.5f;

    /// <summary>
    /// パーセンテージ点滅演出
    /// </summary>
    IEnumerator MatchTextBlinking()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(dispTime);
            matchRateText.enabled = false;
            yield return new WaitForSeconds(unDispTime);
            matchRateText.enabled = true;
        }
    }

    /// <summary>
    /// デバッグ用　実行フェーズを再度行う
    /// </summary>
    public void DebugPlayRetry()
    {
        IsDebug = true;

        PlayPhaseEnd();
        stageController.ToPlayPhase();
    }
}
