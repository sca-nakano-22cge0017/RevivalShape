using System.Collections;
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

    [SerializeField, Header("ボタン表示範囲")] Vector2 buttonRange;

    [SerializeField] GameObject selectPhaseUI;

    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ

    int[,] playerInputData;
    int selectShapeNum = 1;  // 選択中の図形

    ShapeData.Shape[] shapeType;     // 使用図形の種類

    bool firstInput = true; // 一番最初の入力かどうか

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer; // プレイヤーの解答

    // 削除モード
    [SerializeField] GameObject eraserModeButton;
    public bool IsEraser { get; set; } = false;

    // 確認モード
    [SerializeField] GameObject checkModeButton;
    [SerializeField] GameObject checkModeWindow;
    [SerializeField] GameObject stepsParent;
    [SerializeField] GameObject stepsPrefab;
    Image[] steps;
    [SerializeField] Sprite[] shapeIcon;
    public bool IsCheck { get; set; } = false;
    Vector2 checkPos = new Vector2(0, 0); // 確認するマス

    // 確認カメラモードのウィンドウを消去できるか
    bool canCheckWindowUnDisp = false;

    // スワイプして数字を上昇させられるか
    public bool CanSwipInput { get; set; } = false;

    // 各モード時の黒背景
    [SerializeField] GameObject modeBG;

    [SerializeField] GameObject clearBG;

    // 図形変更
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // ウィンドウ、UI非表示
        selectPhaseUI.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);
        clearBG.SetActive(false);

        // 図形変更ボタンの非表示
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
    }

    private void Update()
    {
        UIControl();
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
            InputDataToButton();
            return;
        }

        // マップサイズ取得
        mapSize = stageController.MapSize;

        ArraysCreate();
        UISetting();

        firstInput = false;
    }

    /// <summary>
    /// 選択フェーズ終了
    /// </summary>
    public void SelectPhaseEnd()
    {
        InputNumSave();

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
        // 配列　要素数指定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        steps = new Image[(int)mapSize.y];

        InputNumDataInitialize();
        ShapeArrayInitialize();
    }

    /// <summary>
    /// UIのサイズなどを設定
    /// </summary>
    void UISetting()
    {
        // ボタンの親オブジェクトのサイズ調整
        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonRange.x, buttonRange.y);
        buttonParent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(buttonRange.x / mapSize.x, buttonRange.y / mapSize.z);

        // マップの広さ分ボタンを生成
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                var button = Instantiate(buttonPrefab, buttonParent.transform);

                selectButtons[x, z] = button.GetComponent<SelectPhaseButton>();
                selectButtons[x, z].Position = new Vector2(x, z);
                selectButtons[x, z].Input_max = (int)mapSize.y;
            }
        }

        // 確認カメラモードのウィンドウ生成
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // 作ったらHierarchyの一番上に設定する → 下から順に生成する
            steps[y].transform.SetAsFirstSibling();
        }
    }

    /// <summary>
    /// UI制御
    /// </summary>
    void UIControl()
    {
        if (IsEraser || IsCheck)
        {
            // モード時は背景暗く
            modeBG.SetActive(true);
            clearBG.SetActive(true);

            // ボタンの外を押したらモード終了
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mPos = Input.mousePosition;
                float minX = Screen.width / 2 - buttonRange.x / 2;
                float maxX = Screen.width - minX;
                float minY = Screen.height / 2 - buttonRange.y / 2;
                float maxY = Screen.height - minY;

                if (mPos.x < minX || mPos.x > maxX || mPos.y < minY || mPos.y > maxY)
                {
                    if (IsEraser) IsEraser = false;
                    if (IsCheck) IsCheck = false;
                }
            }
        }
        else
        {
            modeBG.SetActive(false);
            clearBG.SetActive(false);
        }

        // ボタンのレイヤー位置調整
        if (IsEraser)
        {
            checkModeButton.transform.SetAsFirstSibling();
            eraserModeButton.transform.SetAsLastSibling();
        }
        if (IsCheck)
        {
            eraserModeButton.transform.SetAsFirstSibling();
            checkModeButton.transform.SetAsLastSibling();
        }

        // 画面タップで確認カメラモードのウィンドウを閉じる
        if (canCheckWindowUnDisp && Input.GetMouseButtonDown(0))
        {
            checkModeWindow.SetActive(false);
            canCheckWindowUnDisp = false;

            selectButtons[(int)checkPos.x, (int)checkPos.y].IsCheck = false;
        }
    }

    /// <summary>
    /// 消去モードと通常モードを切り替え
    /// </summary>
    public void EraserModeChange()
    {
        IsEraser = !IsEraser;
    }

    /// <summary>
    /// 確認モードと通常モードを切り替え
    /// </summary>
    public void CheckModeChange()
    {
        IsCheck = !IsCheck;
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

        checkPos = new Vector2(0, 0); // 確認するマス
        bool isSearchEnd = false;

        // 確認するマスがどれかを調べる
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // 確認するマスが見つかったらfor文を抜ける
                if (selectButtons[x, z].IsCheck)
                {
                    checkPos = new Vector2(x, z);
                    isSearchEnd = true;
                    break;
                }
            }

            if (isSearchEnd) break;
        }

        for (int i = 0; i < steps.Length; i++)
        {
            // 配置されている図形を取得
            var shape = (int)playerAnswer[(int)checkPos.x, i, (int)checkPos.y];

            Sprite s = shapeIcon[shape];  // 画像取得
            steps[i].sprite = s;          // 画像変更
        }

        StartCoroutine(CheckWindowUnDisp());
    }

    /// <summary>
    /// ウィンドウの表示と非表示が一瞬で行われるので表示後一定時間待ってから、非表示にできるようにする
    /// </summary>
    IEnumerator CheckWindowUnDisp()
    {
        yield return new WaitForSeconds(0.1f);
        canCheckWindowUnDisp = true;
    }

    /// <summary>
    /// 選択中の図形を変更 ボタンで呼び出す
    /// </summary>
    /// <param name="shape">選択する図形の名前</param>
    public void ShapeChange(string shapeName)
    {
        // 列挙型を全検索
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // 名前が打ち込まれたstringと同じなら、選択している図形を変更する
            if (name.ToLower() == shapeName.ToLower())
            {
                selectShapeNum = (int)value;
            }
        }
    }

    /// <summary>
    /// 図形変更ボタンの表示・非表示
    /// 使わない図形は非表示にする
    /// </summary>
    void ShapeChangeButtonsSet()
    {
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

                    if (firstShape)
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
                //playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
                playerInputData[z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// 入力データを初期化
    /// </summary>
    /// <param name="num">初期化する配列</param>
    void InputNumDataInitialize()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                playerInputData[x, z] = 0;
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
                selectButtons[x, z].InputNum = playerInputData[z, x];
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
    /// 図形入力
    /// </summary>
    /// <param name="buttonPos">入力するマス</param>
    public void ShapeInput(Vector2 buttonPos)
    {
        // 次にオブジェクトを配置する位置
        int nextYPos = 0;

        // 空白マスを検索する 1段目から検索していき、空白マスがあったら次の図形をそこから上へ配置していく
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            if (playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] != ShapeData.Shape.Empty) continue;
            else
            {
                nextYPos = y;
                break;
            }
        }

        playerAnswer[(int)buttonPos.x, nextYPos, (int)buttonPos.y] = (ShapeData.Shape)selectShapeNum;
    }

    /// <summary>
    /// 削除モード　図形削除
    /// </summary>
    /// <param name="buttonPos">削除するマス</param>
    public void ShapeDelete(Vector2 buttonPos)
    {
        // 上から順に削除
        for (int y = (int)mapSize.y - 1; y >= 0; y--)
        {
            // 空白マスは飛ばす
            if (playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] == ShapeData.Shape.Empty) continue;
            else
            {
                playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] = ShapeData.Shape.Empty;
                break;
            }
        }
    }
}
