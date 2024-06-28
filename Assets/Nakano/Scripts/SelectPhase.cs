using System.Collections;
using UnityEngine;
using System;
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
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField, Header("ボタン表示範囲")] Vector2 buttonRange;

    [SerializeField] GameObject selectPhaseUI;

    SelectPhaseButton[,] selectButtons; // 各ボタンのデータ

    int[,] playerInputData;
    ShapeData.Shape selectingShape; // 選択中の図形

    ShapeData.Shape[] shapeType;     // 使用図形の種類

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer; // プレイヤーの解答

    // 各図形毎のUI
    [SerializeField] select.ShapesUI[] shapesUI;

    // 削除モード
    [SerializeField] GameObject eraserModeButton;
    public bool IsEraser { get; set; } = false;

    // 確認モード
    [SerializeField] GameObject checkModeButton;
    [SerializeField] GameObject checkModeWindow;
    [SerializeField] GameObject stepsParent;
    [SerializeField] GameObject stepsPrefab;
    Image[] steps;
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

    [SerializeField] PlayPhase play;

    private void Awake()
    {
        // ウィンドウ、UI非表示
        selectPhaseUI.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);
        clearBG.SetActive(false);

        // 図形変更ボタンの非表示
        foreach (var ui in shapesUI)
        {
            if(ui.shape == ShapeData.Shape.Empty) continue;

            ui.button.SetActive(false);
        }
    }

    public void Initialize()
    {
        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列　要素数指定
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

        steps = new Image[(int)mapSize.y];

        // 図形変更ボタン　使う図形のボタンのみ表示する
        ShapeChangeButtonsSet();

        UISetting();
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
        stageController.PlayerAnswer = playerAnswer;
    }

    /// <summary>
    /// ボタンの生成、UIのサイズを設定
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
                selectButtons[x, z].selectPhase = this;
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
            if (Input.touchCount >= 1)
            {
                Touch t = Input.GetTouch(0);

                Vector2 min = new Vector2(Screen.width / 2 - buttonRange.x / 2, Screen.height / 2 - buttonRange.y / 2);
                Vector2 max = new Vector2(Screen.width - min.x, Screen.height - min.y);

                if (t.phase == TouchPhase.Began &&
                    !stageController.TapOrDragRange(t.position, min, max))
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
        if (canCheckWindowUnDisp && Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
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
            Sprite s = shapesUI[0].sprite;// 画像取得
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
            var shape = playerAnswer[(int)checkPos.x, i, (int)checkPos.y];

            // 画像変更
            foreach (var ui in shapesUI)
            {
                if(ui.shape == shape)
                {
                    steps[i].sprite = ui.sprite;
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
    public void ShapeChange(string shapeName)
    {
        // 列挙型を全検索
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // 名前が打ち込まれたstringと同じなら、選択している図形を変更する
            if (name.ToLower() == shapeName.ToLower())
            {
                selectingShape = value;
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

        foreach (var ui in shapesUI)
        {
            if(ui.shape == ShapeData.Shape.Empty) continue;

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

    private IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}
