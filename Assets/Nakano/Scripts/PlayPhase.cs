using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 実行フェーズ
/// </summary>
public class PlayPhase : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private TapManager tapManager;
    private Vibration vibration;

    // 親オブジェクト
    [SerializeField] private Transform objParent;

    [SerializeField] private GameObject playPhaseUI;

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

    [SerializeField] private GameObject clearWindow;

    /// <summary>
    /// 確認中か
    /// </summary>
    public bool IsDebug { get; private set; } = false;

    private void Awake()
    {
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;

        vibration = GameObject.FindObjectOfType<Vibration>();
    }

    public void Initialize()
    {
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

    private void Update()
    {
        if (isFalling)
        {
            Skip();
            FastForward();
        }
    }

    /// <summary>
    /// 落下演出スキップ
    /// </summary>
    void Skip()
    {
        tapManager.DoubleTap(() =>
        {
            isSkip = true;
            IsFastForward = false;
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

    /// <summary>
    /// 実行フェーズ移行時の処理
    /// </summary>
    public void PlayPhaseStart()
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

    /// <summary>
    /// 実行フェーズ終了
    /// </summary>
    public void PlayPhaseEnd()
    {
        StopAllCoroutines();
        matchRateText.enabled = false;
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        // ブロック削除
        Transform children = objParent.GetComponentInChildren<Transform>();
        foreach (Transform obj in children)
        {
            obj.gameObject.SetActive(false);
        }
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
                        if (map[x, y, z] != ShapeData.Shape.Empty) mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                    }
                    else
                    {
                        if (mapObj[x, y, z])
                        {
                            mapObj[x, y, z].transform.position = pos;
                            mapObj[x, y, z].SetActive(true);
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
        isFalling = true;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    // 透明ブロックなら演出を飛ばす
                    if (map[x, y, z] == ShapeData.Shape.Alpha)
                    {
                        mapObj[x, y, z].transform.position = new Vector3(-x, y, z);
                        continue;
                    }

                    if (mapObj[x, y, z])
                    {
                        mapObj[x, y, z].GetComponent<ShapeObjects>().TargetHeight = y;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().FallSpeed = fallSpeed;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsFall = true;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = true;
                    }

                    if (!isSkip)
                        yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        isFalling = false;

        yield return new WaitForSeconds(fallToMatchdispTime);
        isSkip = false;
        IsFastForward = false;

        // 全体の一致率算出
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);
        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if (matchRate >= 100)
        {
            clearWindow.SetActive(true);

            vibration.PluralVibrate(2, (long)(clearVibrateTime * 1000));

            stageController.IsClear = true;
        }
        else
        {
            StartCoroutine(MatchTextBlinking());
            stageController.IsRetry = true;
        }
    }

    const float unDispTime = 0.3f;
    const float dispTime = 0.5f;

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