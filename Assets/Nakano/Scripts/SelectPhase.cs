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

    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ

    int[,,] _playerInputData; // 入力データ
    int selectShapeNum = 1; // 選択中の図形

    ShapeData.Shape[] shapeType; // 使用図形の種類
    private int shapeTypeAmount = 0; // 使用図形の種類数

    bool firstInput = true; // 一番最初の入力かどうか

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    // 消去モード
    [SerializeField] GameObject eraserModeWindow;
    bool isEraser = false;
    public bool IsEraser { get { return isEraser; } }

    // 図形変更
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // ウィンドウ非表示
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);

        // 図形変更ボタンの非表示
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
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

        // 図形変更ボタン　使う図形のボタンのみ表示する
        ShapeChangeButtonsSet();

        selectShapeNum = 1;

        if (!firstInput)
        {
            // 解答初期化
            ShapeArrayInitialize();
            stageController.PlayerAnswer = playerAnswer;

            InputDataToButton();
            return;
        }

        ArraysCreate();
        ButtonsCreate();

        firstInput = false;
    }

    /// <summary>
    /// 選択フェーズ終了
    /// </summary>
    public void SelectPhaseEnd()
    {
        InputNumSave();
        IntArrayToShapeArray();

        // ウィンドウを閉じる
        selectPhaseUI.SetActive(false);

        // プレイヤーの答えを保存
        stageController.PlayerAnswer = playerAnswer;
    }

    /// <summary>
    /// 配列要素数指定
    /// </summary>
    void ArraysCreate()
    {
        // マップサイズ取得
        mapSize = stageController.MapSize;

        shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

        // 配列　要素数指定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        _playerInputData = new int[shapeTypeAmount, (int)mapSize.x, (int)mapSize.z];
        InputNumDataInitialize();

        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize.x * 200, mapSize.z * 200);

        ShapeArrayInitialize();
    }

    /// <summary>
    /// ボタンの生成
    /// </summary>
    void ButtonsCreate()
    {
        // マップの広さ分ボタンを生成
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
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

    /// <summary>
    /// 選択中の図形を変更 ボタンで呼び出す
    /// </summary>
    /// <param name="shape">選択する図形の名前</param>
    public void ShapeChange(string shapeName)
    {
        int n = selectShapeNum;
        ShapeData.Shape shape = (ShapeData.Shape)n;

        InputNumSave(); // 入力データ保存

        // 列挙型を全検索
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // 打ち込まれたstringと同じなら変更する
            if(name.ToLower() == shapeName.ToLower())
            {
                selectShapeNum = (int)value;
                InputDataToButton();
            }
        }
    }

    /// <summary>
    /// 図形変更ボタンの表示・非表示
    /// 使わない図形は非表示にする
    /// </summary>
    void ShapeChangeButtonsSet()
    {
        shapeType = stageController.ShapeType; // 使用図形を取得

        for(int b = 0; b < shapeChangeButtons.Length; b++)
        {
            string buttonsName = shapeChangeButtons[b].name;
            buttonsName.ToLower();

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);
                needShapeName.ToLower();
                
                if (buttonsName == needShapeName)
                {
                    shapeChangeButtons[b].SetActive(true);
                    break;
                }
            }
        }
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
                _playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// 入力データを初期化
    /// </summary>
    /// <param name="num">初期化する配列</param>
    void InputNumDataInitialize()
    {
        for(int n = 0; n < shapeTypeAmount; n++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    _playerInputData[n, x, z] = 0;
                }
            }
        }
    }

    /// <summary>
    /// ボタンの表示を変更
    /// </summary>
    /// <param name="isReset">値を0にするか</param>
    void InputDataToButton()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z].InputNum = _playerInputData[selectShapeNum, z, x];
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
        ShapeData.Shape shape;

        for (int n = 0; n < shapeTypeAmount; n++)
        {
            shape = (ShapeData.Shape) n;

            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    int nextYPos = 0;

                    // 空白マスを検索する 1段目から検索していき、空白マスがあったら次の図形をそこから上へ配置していく
                    for (int y = 0; y < (int)mapSize.y; y++)
                    {
                        if (playerAnswer[x, y, z] != ShapeData.Shape.Empty) continue;
                        else
                        {
                            nextYPos = y;
                            break;
                        }
                    }

                    // playerInputDataにはY軸方向に積む数が入っている
                    for (int y = 0; y < _playerInputData[n, z, x]; y++)
                    {
                        playerAnswer[x, y + nextYPos, z] = shape;
                    }
                }
            }
        }
    }
}
