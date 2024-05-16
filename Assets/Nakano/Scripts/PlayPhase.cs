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
    [SerializeField] Vibration vibration;

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
    int matchRate = 0; // 一致率

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;
    bool isClear = false;
    bool isRetry = false;

    private void Awake()
    {
        // UI等を消しておく
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;
    }

    private void Update()
    {
        if (isClear && Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("SelectScene");
            isClear = false;
        }

        if (isRetry && Input.GetMouseButton(0))
        {
            // 確認フェーズに戻る
            stageController.ToCheckPhase();
            isRetry = false;
        }
    }

    /// <summary>
    /// 実行フェーズ移行時の処理
    /// </summary>
    public void PlayPhaseStart()
    {
        playPhaseUI.SetActive(true);

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
        matchRate = 0;

        AnswerInstance();
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

                bool hasAnything = false; // 正答/プレイヤーの解答のどちらかで何か置かれているマスか

                for (int y = 0; y < mapSize.y; y++)
                {
                    // 生成
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                    
                    // 正答とプレイヤーの解答を比べる
                    if (map[x, y, z] != ShapeData.Shape.Empty) total++;
                    if(correctAnswer[x, y, z] != ShapeData.Shape.Empty) c_total++;

                    // 超過分 正答では何も置かれていない場所にオブジェクトが置かれた場合
                    if(correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
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

                if(hasAnything) hasBlockTrout++;

                // 1マスあたりの一致率を計算
                if(excess > 0) // 超過がある場合
                {
                    matchRateTrout = (float)(1 - ((total - (c_total - diff)) / total));
                    // 1 - (( 1マス内の総ブロック数 ) - (( 正答の1マス内のブロック数 ) - 相違数 ) / 1マス内の総ブロック数 )
                }
                else
                {
                    if(c_total > 0)
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
                    StartCoroutine(VibrateOn(mapObj[x, y, z]));
                    yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        yield return new WaitForSeconds(fallToMatchdispTime);
        MatchRateDisp();
    }

    IEnumerator VibrateOn(GameObject obj)
    {
        yield return new WaitForSeconds(0.1f);
        obj.GetComponent<ShapeObjects>().IsVibrate = true; // 振動オン
    }

    /// <summary>
    /// プレイヤーの解答と正答の一致率を確認・表示
    /// </summary>
    void MatchRateDisp()
    {
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);

        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if(matchRate >= 100)
        {
            clearWindow.SetActive(true);
            missionCheck.Mission();

            vibration.PluralVibrate(2, 500);
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
}
