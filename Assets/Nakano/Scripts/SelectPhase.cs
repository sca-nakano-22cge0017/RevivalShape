using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択フェーズ
/// </summary>
public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField] GameObject selectPhaseUI;

    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ

    int[,,] playerInputData; // 入力データ [図形の種類, X座標, Z座標]
    int selectShapeNum = 1;  // 選択中の図形

    ShapeData.Shape[] shapeType;     // 使用図形の種類
    private int shapeTypeAmount = 0; // 使用図形の種類数

    bool firstInput = true; // 一番最初の入力かどうか

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer; // プレイヤーの解答

    // 消去モード
    [SerializeField] GameObject eraserMode;
    public bool IsEraser { get; private set; } = false;

    // 確認モード
    [SerializeField] GameObject checkModeWindow;
    [SerializeField] Image[] steps;
    [SerializeField] Sprite[] shapeIcon;
    public bool IsCheck { get; set; } = false;

    // 各モード時の黒背景
    [SerializeField] GameObject modeBG;

    // 図形変更
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // ウィンドウ、UI非表示
        selectPhaseUI.SetActive(false);
        eraserMode.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);

        // 図形変更ボタンの非表示
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
    }

    private void Update()
    {
        // モード時は背景暗く
        if(IsEraser || IsCheck) modeBG.SetActive(true);
        else modeBG.SetActive(false);
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

        // 初回でないとき
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

        // 配列　要素数指定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[shapeTypeAmount, (int)mapSize.x, (int)mapSize.z];

        // 親オブジェクトのサイズ調整
        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize.x * 200, mapSize.z * 200);

        InputNumDataInitialize();
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
    public void EraserModeChange()
    {
        if (IsCheck) return; // 確認モードなら無効

        IsEraser = !IsEraser;

        if (IsEraser) eraserMode.SetActive(true);
        else eraserMode.SetActive(false);
    }

    /// <summary>
    /// 確認モードと通常モードを切り替え
    /// </summary>
    public void CheckModeChange()
    {
        if (IsEraser) return; // 消去モードなら無効

        IsCheck = !IsCheck;

        if (!IsCheck) checkModeWindow.SetActive(false);
    }

    /// <summary>
    /// 確認カメラモード 指定したマスの図形を表示する
    /// </summary>
    public void CheckWindowDisp()
    {
        // ウィンドウ表示
        checkModeWindow.SetActive(true);

        // 画像初期化
        for (int i = 0; i < steps.Length; i++)
        {
            Sprite s = shapeIcon[0]; // 画像取得
            steps[i].sprite = s; // 画像変更
        }

        InputNumSave();
        IntArrayToShapeArray();

        Vector2 pos = new Vector2(0, 0); // 確認するマス
        bool isSearchEnd = false;

        // 確認するマスがどれかを調べる
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // 確認するマスが見つかったらfor文を抜ける
                if(selectButtons[x, z].IsCheck)
                {
                    pos = new Vector2(x, z);
                    isSearchEnd = true;
                    break;
                }
            }

            if(isSearchEnd) break;
        }

        for(int i = 0; i < steps.Length; i++)
        {
            // 配置されている図形を取得
            var shape = (int)playerAnswer[(int)pos.x, i, (int)pos.y];

            Sprite s = shapeIcon[shape];  // 画像取得
            steps[i].sprite = s;          // 画像変更
        }
    }

    /// <summary>
    /// 選択中の図形を変更 ボタンで呼び出す
    /// </summary>
    /// <param name="shape">選択する図形の名前</param>
    public void ShapeChange(string shapeName)
    {
        InputNumSave(); // 入力データ保存

        // 列挙型を全検索
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // 名前が打ち込まれたstringと同じなら、選択している図形を変更する
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
        // 使用している図形の種類数を取得
        shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

        // 使用図形を取得
        shapeType = stageController.ShapeType;

        bool firstShape = true; // 一番最初に配置する図形

        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            // ボタンの名前を取得
            string buttonsName = shapeChangeButtons[b].name;
            buttonsName.ToLower();

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);
                needShapeName.ToLower();
                
                // ボタンの名前と使用している図形の種類が被っているものがあれば
                if (buttonsName == needShapeName)
                {
                    // ボタンを表示
                    shapeChangeButtons[b].SetActive(true);

                    if(firstShape)
                    {
                        // 初期状態で選択している図形を設定
                        selectShapeNum = (int)shapeType[st];
                        firstShape = false;
                    }

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
                playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
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
                    playerInputData[n, x, z] = 0;
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
                // 入力されていた値をボタンに反映する
                selectButtons[x, z].InputNum = playerInputData[selectShapeNum, z, x];
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
                    // 空を入れておく
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
        // 一度初期化する
        ShapeArrayInitialize();

        ShapeData.Shape shape;

        for (int n = 0; n < shapeTypeAmount; n++)
        {
            shape = (ShapeData.Shape) n;

            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    // 次にオブジェクトを配置する位置
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
                    for (int y = 0; y < playerInputData[n, z, x]; y++)
                    {
                        playerAnswer[x, y + nextYPos, z] = shape;
                    }
                }
            }
        }
    }
}
