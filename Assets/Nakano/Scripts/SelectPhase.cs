using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField] GameObject selectPhaseUI;

    bool buttonsCreated = false; // ボタン作成済みかどうか
    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ
    int[,] playerInputData; // 入力データ

    //! 制作中
    List<SelectPhaseButton[,]> _selectButtons; // 各ボタンのデータ
    ShapeData.Shape selectShape; // 選択中の図形

    bool arraysCreated = false; // 配列の要素数を指定済みかどうか
    bool firstInput = true; // 一番最初の入力かどうか

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    // 消去モード
    [SerializeField] GameObject eraserModeWindow;
    bool isEraser = false;
    public bool IsEraser { get { return isEraser; } }

    private void Awake()
    {
        // ウィンドウ非表示
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);
    }

    private void Update()
    {
        if (isEraser) eraserModeWindow.SetActive(true);
        else eraserModeWindow.SetActive(false);
    }

    /// <summary>
    /// 選択フェーズ移行時の処理
    /// </summary>
    public void SelectPhaseStart()
    {
        // UI表示
        selectPhaseUI.SetActive(true);

        ArraysCreate();
        
        ButtonsCreate();
    }

    /// <summary>
    /// 選択フェーズ終了
    /// </summary>
    public void SelectPhaseEnd()
    {
        InputNumSave();

        InputNumReset();

        IntArrayToShapeArray();

        // ウィンドウを閉じる
        //buttonParent.SetActive(false);
        selectPhaseUI.SetActive(false);

        // プレイヤーの答えを保存
        stageController.PlayerAnswer = playerAnswer;
        firstInput = false;
    }

    /// <summary>
    /// 配列要素数指定
    /// </summary>
    void ArraysCreate()
    {
        if(arraysCreated) return;

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列　要素数指定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize.x * 200, mapSize.z * 200);

        ShapeArrayInitialize();

        arraysCreated = true;
    }

    /// <summary>
    /// ボタンの生成
    /// </summary>
    void ButtonsCreate()
    {
        if(buttonsCreated) return;

        // マップの広さ分ボタンを生成
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
            }
        }

        buttonsCreated = true;
    }

    /// <summary>
    /// ボタンに入力した数値を配列に保存する
    /// </summary>
    void InputNumSave()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // 入力した数をint型の配列に代入
                playerInputData[z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// ボタンに入力した数値を全て0に戻す
    /// </summary>
    void InputNumReset()
    {
        if (!firstInput) return;

        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // 初回なら0を入れておく
                selectButtons[x, z].InputNum = 0;
            }
        }
    }

    /// <summary>
    /// 配置データの初期化
    /// </summary>
    void ShapeArrayInitialize()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                for (int y = 0; y < (int)mapSize.y; y++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
    }

    /// <summary>
    /// int型の配列からShape型の三次元配列に変換する
    /// </summary>
    void IntArrayToShapeArray()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // playerInputDataにはY軸方向に積む数が入っている
                for (int y = 0; y < playerInputData[z, x]; y++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Cube;
                }
            }
        }
    }

    /// <summary>
    /// 消去モードと通常モードを切り替え
    /// </summary>
    public void EraserMode()
    {
        isEraser = !isEraser;
    }
}
