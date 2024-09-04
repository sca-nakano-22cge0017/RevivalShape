using UnityEngine;
using UnityEngine.UI;
using static Extensions;

namespace select
{
    /// <summary>
    /// 各図形毎の2D画像、図形変更時のボタン
    /// </summary>
    [System.Serializable]
    public class ShapesUI
    {
        public ShapeData.Shape shape; // 図形
        public Sprite sprite;         // アイコンに使用する画像
        public GameObject button;     // 図形変更ボタン
    }
}

/// <summary>
/// 選択フェーズ
/// </summary>
public class SelectPhase : MonoBehaviour, IPhase
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;

    // 入力ボタン
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private RectTransform buttonParent_rc;
    [SerializeField] private GridLayoutGroup buttonParent_glg;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField, Header("ボタン表示範囲")] private Vector2 buttonRange;

    [SerializeField] private GameObject selectPhaseUI;

    private SelectPhaseButton[,] selectButtons; // 各ボタンに入力されたデータ
    private int[,] playerInputData;             // プレイヤーが入力したデータ

    private ShapeData.Shape selectingShape;     // 選択中の図形
    [SerializeField, Header("選択中の図形")] Image selectingShapeImage;
    private ShapeData.Shape[] shapeType;        // 使用図形の種類

    private Vector3 mapSize;

    private ShapeData.Shape[,,] playerAnswer;   // プレイヤーの解答

    // 各図形毎のUI
    [SerializeField] private select.ShapesUI[] shapesUI;

    // 削除モード
    [SerializeField, Header("削除モードのボタン")] private GameObject eraserModeButton;
    [SerializeField] private Image eraserModeButton_img;
    [SerializeField, Header("削除モードのウィンドウ")] private GameObject eraserModeWindow;
    /// <summary>
    /// 削除モードか
    /// </summary>
    public bool IsEraser { get; set; } = false;
    private bool canEraserWindowUnDisp = false;

    // 確認モード
    [SerializeField, Header("確認モードのボタン")] private GameObject checkModeButton;
    [SerializeField] private Image checkModeButton_img;
    [SerializeField, Header("確認モードのウィンドウ")] private GameObject checkModeWindow;
    [SerializeField] private GameObject stepsParent;       // 画像を配置する場所
    [SerializeField] private RectTransform stepsParent_rc; // stepsParentのRectTransform
    [SerializeField] private GameObject stepsPrefab;       // 生成する画像
    private Image[] steps;                                 // 表示する画像　Spriteを変更する
    /// <summary>
    /// 確認モードか
    /// </summary>
    public bool IsCheck { get; private set; } = false;
    private Vector2 checkPos = Vector2.zero;    // 確認するマスの座標

    // 確認カメラモードのウィンドウを消去できるか
    private bool canCheckWindowUnDisp = false;

    private bool canSwipInput = false;
    /// <summary>
    /// スワイプして数字を上昇させられるか
    /// </summary>
    public bool CanSwipInput
    {
        get
        {
            return canSwipInput;
        }
        set
        {
            canSwipInput = value;
        }
    }

    private bool isLongTap = false;
    /// <summary>
    /// ボタンを長押ししているか
    /// </summary>
    public bool IsLongTap { get { return isLongTap; } set { isLongTap = value; } }

    // 全消し
    [SerializeField, Header("全消し確認ウィンドウ")] private GameObject confirmWindow;
    [SerializeField, Header("全消しを機能させるか")] private bool canReset = true;

    [SerializeField, Header("各モード時の黒背景")] private GameObject modeBG;

    public void Initialize()
    {
        // ウィンドウ、UI非表示
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);
        checkModeWindow.SetActive(false);
        confirmWindow.SetActive(false);
        modeBG.SetActive(false);

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 使用図形を取得
        shapeType = stageController.ShapeType;

        // ボタンの親オブジェクトのサイズ調整
        buttonParent_rc.sizeDelta = new Vector2(buttonRange.x, buttonRange.y);
        buttonParent_glg.cellSize = new Vector2(buttonRange.x / mapSize.x, buttonRange.y / mapSize.z);

        // 配列の要素数設定
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        // 配列の初期化
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerInputData[x, z] = 0;
                }
            }
        }

        // マップの広さ分ボタンを生成
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                var button = Instantiate(buttonPrefab, buttonParent.transform);

                selectButtons[x, z] = button.GetComponent<SelectPhaseButton>();
                selectButtons[x, z].Position = new Vector2(x, z);
                selectButtons[x, z].InputMax = (int)mapSize.y;
                selectButtons[x, z].selectPhase = this;
            }
        }

        // 確認カメラモードのウィンドウ設定
        steps = new Image[(int)mapSize.y];
        stepsParent_rc.transform.localPosition = new Vector2(0, 1105 + (mapSize.y - 4) * 110);
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // 作ったらHierarchyの一番上に設定する → 下から順に生成する
            steps[y].transform.SetAsFirstSibling();
        }

        // 図形変更ボタン設定
        bool firstShape = true;
        foreach (var ui in shapesUI)
        {
            if (ui.shape == ShapeData.Shape.Empty) continue;

            string buttonsName = ui.button.name;

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);

                // ボタンの名前と使用している図形の種類が被っているものがあれば
                if (buttonsName.ToLower() == needShapeName.ToLower())
                {
                    // ボタンを表示
                    ui.button.SetActive(true);

                    // 一番目の図形なら
                    if (firstShape)
                    {
                        // 初期状態で選択している図形を設定
                        selectingShape = shapeType[st];
                        firstShape = false;
                    }

                    break;
                }
                else ui.button.SetActive(false);
            }
        }

        ShapeChangeButtonDispSet();
    }

    /// <summary>
    /// 選択フェーズ移行時の処理
    /// </summary>
    public void PhaseStart()
    {
        // UI表示
        selectPhaseUI.SetActive(true);

        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // 入力されていた値をボタンに反映する
                selectButtons[x, z].InputNum = playerInputData[z, x];
            }
        }
    }

    public void PhaseUpdate()
    {
        // 画面タップで確認モードのウィンドウを閉じる
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (canCheckWindowUnDisp)
            {
                checkModeWindow.SetActive(false);
                canCheckWindowUnDisp = false;

                selectButtons[(int)checkPos.x, (int)checkPos.y].ShapeCheckEnd();
            }

            if (canEraserWindowUnDisp)
            {
                eraserModeWindow.SetActive(false);
                canEraserWindowUnDisp = false;
            }
        }
    }

    /// <summary>
    /// 選択フェーズ終了
    /// </summary>
    public void PhaseEnd()
    {
        InputNumSave();

        // ウィンドウを閉じる
        selectPhaseUI.SetActive(false);

        // プレイヤーの答えを保存
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    stageController.PlayerAnswer[x, y, z] = playerAnswer[x, y, z];
                }
            }
        }
    }

    Color clear = new Color(1, 1, 1, 0);
    Color unClear = new Color(1, 1, 1, 1);

    /// <summary>
    /// 各モードの表示設定
    /// </summary>
    void ModeUIDispSet()
    {
        if (IsEraser || IsCheck)
        {
            // モード時は背景暗く
            modeBG.SetActive(true);

            Image disp = null;
            Image unDisp = null;

            if (IsCheck)
            {
                disp = checkModeButton_img;
                unDisp = eraserModeButton_img;
            }
            if (IsEraser)
            {
                unDisp = checkModeButton_img;
                disp = eraserModeButton_img;
            }

            // モード中はボタンを白く光らせる
            disp.color = unClear;
            unDisp.color = clear;

            // 表示順変更
            unDisp.transform.SetAsFirstSibling();
            disp.transform.SetAsLastSibling();
        }
        else
        {
            checkModeButton_img.color = clear;
            eraserModeButton_img.color = clear;

            modeBG.SetActive(false);
        }

        if (IsEraser && !eraserModeWindow.activeSelf && canReset)
            eraserModeWindow.SetActive(true);
        if (!IsEraser && eraserModeWindow.activeSelf && canReset)
            eraserModeWindow.SetActive(false);
    }

    /// <summary>
    /// 消去モードと通常モードを切り替え
    /// </summary>
    public void EraserModeChange()
    {
        IsEraser = !IsEraser;
        ModeUIDispSet();
    }

    /// <summary>
    /// 確認モードと通常モードを切り替え
    /// </summary>
    public void CheckModeChange()
    {
        IsCheck = !IsCheck;
        ModeUIDispSet();
    }

    public void ModeEnd()
    {
        if (IsEraser) IsEraser = false;
        if (IsCheck) IsCheck = false;

        ModeUIDispSet();
    }

    /// <summary>
    /// 確認カメラモード 指定したマスの図形を表示する
    /// </summary>
    public void CheckWindowDisp(Vector2 _checkPos)
    {
        checkPos = _checkPos;

        // ウィンドウ表示
        checkModeWindow.SetActive(true);

        // 画像初期化
        for (int i = 0; i < steps.Length; i++)
        {
            Sprite s = shapesUI[0].sprite;// 画像取得
            steps[i].sprite = s; // 画像変更
        }

        InputNumSave();

        for (int i = 0; i < steps.Length; i++)
        {
            // 配置されている図形を取得
            var shape = playerAnswer[(int)checkPos.x, i, (int)checkPos.y];

            // 画像変更
            for (int ui = 0; ui < shapesUI.Length; ui++)
            {
                if(shape == shapesUI[ui].shape)
                {
                    steps[i].sprite = shapesUI[ui].sprite;
                }
            }
        }

        // ウィンドウの表示と非表示が一瞬で行われるので表示後一定時間待ってから、非表示にできるようにする
        StartCoroutine(DelayCoroutine(0.1f, () =>
        {
            canCheckWindowUnDisp = true;
        }));
    }

    /// <summary>
    /// 選択中の図形を変更 ボタンで呼び出す
    /// </summary>
    /// <param name="shape">選択する図形の名前</param>
    public void ShapeChange(string shapeType)
    {
        // 列挙型を全検索
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // 名前が打ち込まれたstringと同じなら、選択している図形を変更する
            if (name.ToLower() == shapeType.ToLower())
            {
                selectingShape = value;
            }
        }

        ShapeChangeButtonDispSet();
    }

    /// <summary>
    /// 図形変更ボタンの表示変更
    /// </summary>
    void ShapeChangeButtonDispSet()
    {
        foreach (select.ShapesUI s in shapesUI)
        {
            if (s.shape == ShapeData.Shape.Empty) continue;

            // 選択中の図形は枠表示＋明るくする
            var frame = s.button.transform.Find("Frame").gameObject;
            var front = s.button.transform.Find("Front").gameObject;

            if (s.shape == selectingShape)
            {
                frame.SetActive(true);
                front.SetActive(false);
            }
            else
            {
                frame.SetActive(false);
                front.SetActive(true);
            }
        }

        SelectingShapeDisp();
    }

    /// <summary>
    /// 選択中の図形を表示
    /// </summary>
    void SelectingShapeDisp()
    {
        for (int i = 0; i < shapesUI.Length; i++)
        {
            if (selectingShape == shapesUI[i].shape)
            {
                selectingShapeImage.sprite = shapesUI[i].sprite;
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
                playerInputData[z, x] = selectButtons[x, z].InputNum;
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

        // 解答に保存
        playerAnswer[(int)buttonPos.x, nextYPos, (int)buttonPos.y] = selectingShape;
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

    /// <summary>
    /// 全消し確認ウィンドウの表示/非表示
    /// </summary>
    /// <param name="_isDisp">trueで表示</param>
    public void ResetConfirmWindow(bool _isDisp)
    {
        confirmWindow.SetActive(_isDisp);
    }

    /// <summary>
    /// 全消し
    /// </summary>
    public void DataReset()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerInputData[x, z] = 0;
                    selectButtons[x, z].NumReset();
                }
            }
        }

        ModeEnd();
    }
}
