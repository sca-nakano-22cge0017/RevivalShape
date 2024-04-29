using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ
    int[,] playerInputData; // 入力データ

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    void Start()
    {
        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列　要素数指定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        // マップの広さ分ボタンを生成
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
            }
        }
    }

    void Update()
    {
        // debug
        if(Input.GetKeyDown(KeyCode.Return))
        {
            InputNumSave();
        }
    }

    void InputNumSave()
    {
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                // 入力した数をint型の配列に代入
                playerInputData[x, z] = selectButtons[x, z].InputNum;
            }
        }
    }
}
