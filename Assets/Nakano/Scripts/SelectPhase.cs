using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace select
{
    /// <summary>
    /// 各図形毎の2D画像、図形変更時のボタン
    /// </summary>
    [System.Serializable]
    public class ShapesUI
    {
        public ShapeData.Shape shape;
        public Sprite sprite;
        public GameObject button; // 図形変更ボタン
    }
}

/// <summary>
/// 選択フェーズ
/// </summary>
public class SelectPhase : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;

    [SerializeField] private GameObject buttonParent;
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField, Header("ボタン表示範囲")] private Vector2 buttonRange;

    [SerializeField] private GameObject selectPhaseUI;

    private SelectPhaseButton[,] selectButtons; // 各ボタンのデータ

    private int[,] playerInputData;
    private ShapeData.Shape selectingShape; // 選択中の図形

    private ShapeData.Shape[] shapeType;     // 使用図形の種類

    private Vector3 mapSize;

    private ShapeData.Shape[,,] playerAnswer; // プレイヤーの解答

    // 各図形毎のUI
    [SerializeField] private select.ShapesUI[] shapesUI;

    // 削除モード
    [SerializeField] private GameObject eraserModeButton;
    public bool IsEraser { get; set; } = false;

    // 確認モード
    [SerializeField] private GameObject checkModeButton;
    [SerializeField] private GameObject checkModeWindow;
    [SerializeField] private GameObject stepsParent;
    [SerializeField] private GameObject stepsPrefab;
    private Image[] steps;
    public bool IsCheck { get; private set; } = false;
    private Vector2 checkPos = new Vector2(0, 0); // 確認するマス

    // 確認カメラモードのウィンドウを消去できるか
    private bool canCheckWindowUnDisp = false;

    // スワイプして数字を上昇させられるか
    public bool CanSwipInput { get; set; } = false;

    // 各モード時の黒背景
    [SerializeField] private GameObject modeBG;

    [SerializeField] private GameObject clearBG;

    [SerializeField] private PlayPhase play;

    private void Update()
    {
        if (stageController.phase != StageController.PHASE.SELECT) return;

        if (IsEraser || IsCheck)
        {
            // ボタンの外を押したらモード終了
            if (Input.touchCount >= 1)
            {
                Touch t = Input.GetTouch(0);

                Vector2 min = new Vector2(Screen.width / 2 - buttonRange.x / 2, Screen.height / 2 - buttonRange.y / 2);
                Vector2 max = new Vector2(Screen.width - min.x, Screen.height - min.y);

                if (t.phase == TouchPhase.Began &&
                    !tapManager.TapOrDragRange(t.position, min, max))
                {
                    if (IsEraser) IsEraser = false;
                    if (IsCheck) IsCheck = false;
                }

                ModeUIDispSet();
            }
        }

        // 画面タップで確認カメラモードのウィンドウを閉じる
        if (canCheckWindowUnDisp && Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            checkModeWindow.SetActive(false);
            canCheckWindowUnDisp = false;

            selectButtons[(int)checkPos.x, (int)checkPos.y].ShapeCheckEnd();
        }
    }

    public void Initialize()
    {
        if (!stageController) return;

        // ウィンドウ、UI非表示
        selectPhaseUI.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);
        clearBG.SetActive(false);

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 使用図形を取得
        shapeType = stageController.ShapeType;

        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    selectButtons[x, z] = null;
                    playerInputData[x, z] = 0;
                }
            }
        }

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
                selectButtons[x, z].InputMax = (int)mapSize.y;
                selectButtons[x, z].selectPhase = this;
            }
        }

        // 確認カメラモードのウィンドウ設定
        steps = new Image[(int)mapSize.y];
        stepsParent.GetComponent<RectTransform>().transform.localPosition = new Vector2(0, 900 + mapSize.y * 50);
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // 作ったらHierarchyの一番上に設定する → 下から順に生成する
            steps[y].transform.SetAsFirstSibling();
        }

        // 図形変更ボタン設定
        bool firstShape = true; // 一番最初に配置する図形

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
    public void SelectPhaseStart()
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

    /// <summary>
    /// 選択フェーズ終了
    /// </summary>
    public void SelectPhaseEnd()
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

    void ModeUIDispSet()
    {
        Color clear = new Color(1, 1, 1, 0);
        Color unClear = new Color(1, 1, 1, 1);

        if (IsEraser || IsCheck)
        {
            // モード時は背景暗く
            modeBG.SetActive(true);
            clearBG.SetActive(true);
        }
        else
        {
            checkModeButton.GetComponent<Image>().color = clear;
            eraserModeButton.GetComponent<Image>().color = clear;

            modeBG.SetActive(false);
            clearBG.SetActive(false);
        }

        if (IsEraser)
        {
            checkModeButton.GetComponent<Image>().color = clear;
            eraserModeButton.GetComponent<Image>().color = unClear;

            checkModeButton.transform.SetAsFirstSibling();
            eraserModeButton.transform.SetAsLastSibling();
        }
        if (IsCheck)
        {
            checkModeButton.GetComponent<Image>().color = unClear;
            eraserModeButton.GetComponent<Image>().color = clear;

            eraserModeButton.transform.SetAsFirstSibling();
            checkModeButton.transform.SetAsLastSibling();
        }
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
            foreach (var ui in shapesUI)
            {
                if (ui.shape == shape)
                {
                    steps[i].sprite = ui.sprite;
                }
            }
        }

        // ウィンドウの表示と非表示が一瞬で行われるので表示後一定時間待ってから、非表示にできるようにする
        StartCoroutine(stageController.DelayCoroutine(0.1f, () =>
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
}
