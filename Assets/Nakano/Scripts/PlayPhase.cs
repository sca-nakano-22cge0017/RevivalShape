using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;
    [SerializeField] MissionCheck missionCheck;

    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    ShapeData.Shape[,,] correctAnswer; // 正答

    [SerializeField, Header("オブジェクトを落とす高さ")] int fallPos;
    [SerializeField, Header("オブジェクトを落とす間隔(sec)")] float fallInterval;
    [SerializeField, Tooltip("オブジェクトが全て落下してから一致率表示までの時間(sec)")]
    float fallToMatchdispTime;

    // 一致率計算
    float matchRateTroutTotal = 0; // 1マスあたりの一致率の和
    int hasBlockTrout = 0; // 1つ以上ブロックがあるマスの数
    int c_hasBlockTrout = 0; // 正答の1つ以上ブロックがあるマスの数
    int matchRate = 0; // 一致率

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;
    bool isClear = false;
    bool isRetry = false;

    private void Awake()
    {
        // UI等を消しておく
        objParent.gameObject.SetActive(false);
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;
    }

    private void Update()
    {
        if (isClear && Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("SelectScene");
        }

        if (isRetry && Input.GetMouseButton(0))
        {
            // 確認フェーズに戻る
            stageController.ToCheckPhase();
            PlayPhaseEnd();
        }
    }

    /// <summary>
    /// 実行フェーズ移行時の処理
    /// </summary>
    public void PlayPhaseStart()
    {
        mapSize = stageController.MapSize; // サイズ代入

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
        c_hasBlockTrout = 0;
        matchRate = 0;

        objParent.gameObject.SetActive(true);
        playPhaseUI.SetActive(true);

        AnswerInstance();
    }

    void AnswerInstance()
    {
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        c_hasBlockTrout = 0;
        matchRate = 0;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                float matchRateTrout = 0; // 1マス内の一致率

                int total = 0; // 1マス内のブロックの総数
                int c_total = 0; // 正解の1マス内のブロックの総数
                int excess = 0; // 超過数
                int lack = 0; // 不足数
                int diff = 0; // 相違数

                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

                    if(map[x, y, z] != ShapeData.Shape.Empty) total++;
                    if(correctAnswer[x, y, z] != ShapeData.Shape.Empty) c_total++;

                    // 超過分 正答では何も置かれていない場所にオブジェクトが置かれた場合
                    if(correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        excess++;
                    }

                    // 不足分 正答では何か置かれている場所にオブジェクトが何も置かれていなかった場合
                    else if (correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] == ShapeData.Shape.Empty)
                    {
                        lack++;
                    }

                    // 相違分 形が違う場合
                    else if(correctAnswer[x, y, z] != map[x, y, z] && correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        diff++;
                    }
                }

                if(total > 0) hasBlockTrout++;
                if(c_total > 0) c_hasBlockTrout++;

                // 1マスあたりの一致率を計算
                // 超過がある場合
                if(excess > 0)
                {
                    matchRateTrout = 1 - ((total - (c_total - diff)) / total);
                    // 1 - (( 1マス内の総ブロック数 ) - (( 正答の1マス内のブロック数 ) - 相違数 ) / 1マス内の総ブロック数 )
                }
                else
                {
                    if(c_total > 0)
                        matchRateTrout = (c_total - lack - diff) / c_total;
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

    IEnumerator Fall()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    // 空白マスなら落下演出を飛ばす
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    mapObj[x, y, z].GetComponent<Rigidbody>().constraints =
                        RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        yield return new WaitForSeconds(fallToMatchdispTime);
        MatchRateDisp();
    }

    /// <summary>
    /// プレイヤーの解答と正答の一致率を確認・表示
    /// </summary>
    void MatchRateDisp()
    {
        int hBT = hasBlockTrout > c_hasBlockTrout ? hasBlockTrout : c_hasBlockTrout;
        matchRate = (int)(matchRateTroutTotal / (float)hBT * 100);

        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if(matchRate >= 100)
        {
            clearWindow.SetActive(true);
            missionCheck.Mission();
            isClear = true;
        }
        else
        {
            StartCoroutine(MatchTextBlinking());
            isRetry = true;
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

    public void PlayPhaseEnd()
    {
        // ウィンドウを閉じる
        playPhaseUI.SetActive(false);
    }
}
