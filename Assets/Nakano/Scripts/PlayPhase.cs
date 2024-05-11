using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;

    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    ShapeData.Shape[,,] correctAnswer; // 正答

    [SerializeField, Header("オブジェクトを落とす高さ")] int fallPos;

    // 一致率計算
    int correctObjAmount = 0; // 正答のブロック数
    int matchObjAmount = 0; // 正答と一致している解答ブロック数
    int matchRate = 0; // 一致率

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;
    bool isClear = false;

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

        correctObjAmount = 0;
        matchObjAmount = 0;
        matchRate = 0;

        objParent.gameObject.SetActive(true);
        playPhaseUI.SetActive(true);

        AnswerInstance();
    }

    void AnswerInstance()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

                    // 正答のブロック数をカウント
                    correctObjAmount++;

                    // 正答と一致している解答ブロック数をカウント
                    if (correctAnswer[x, y, z] == map[x, y, z]) matchObjAmount++;
                }
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
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        MatchRateCheck();
    }

    /// <summary>
    /// プレイヤーの解答と正答の一致率を確認・表示
    /// </summary>
    void MatchRateCheck()
    {
        matchRate = (int)((float)matchObjAmount / (float)correctObjAmount * 100);
        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if(matchRate >= 100)
        {
            clearWindow.SetActive(true);
            isClear = true;
        }
        else
        {
            //! テキスト点滅演出
        }
    }
}
