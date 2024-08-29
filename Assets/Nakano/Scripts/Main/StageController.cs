using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Extensions;

/// <summary>
/// メインゲーム制御
/// </summary>
public class StageController : MonoBehaviour
{
    // フェーズ
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = PHASE.CHECK;

    [SerializeField, Header("ステージ名")] private string stageName;
    public string StageName { get { return stageName; } }

    LoadManager loadManager;

    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private MissionScore missionScore;
    [SerializeField] private Tutorial tutorial;

    [SerializeField] private CameraRotate cameraRotate;
    [SerializeField] private SheatCreate sheatCreate;

    // 各フェーズの処理
    [SerializeField] private CheckPhase checkPhase;
    [SerializeField] private SelectPhase selectPhase;
    [SerializeField] private PlayPhase playPhase;

    // UI制御系
    [SerializeField] private TestButton testButton;
    [SerializeField] private GameObject optionButton;

    private bool isTutorial = false;
    /// <summary>
    /// チュートリアルステージかどうか
    /// </summary>
    public bool IsTutorial { get { return isTutorial;} private set { isTutorial = value; } }

    private bool canToCheckPhase = true; // 選択フェーズから確認フェーズに移行できるか
    [SerializeField, Header("選択→確認移行時の確認ウィンドウ")] private GameObject confirmWindow;
    [SerializeField] private Text numberOfReconfirmation;

    /// <summary>
    /// サンプルの大きさ
    /// </summary>
    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // データ取得完了したかどうか
    private bool dataGot = false;

    /// <summary>
    /// ミス回数
    /// </summary>
    public int Miss { get; private set; } = 0;
    
    /// <summary>
    /// 再確認回数
    /// </summary>
    public int Reconfirmation { get; private set; } = 0;

    /// <summary>
    /// 使用図形の種類一覧
    /// </summary>
    public ShapeData.Shape[] ShapeType { get; private set; }

    private ShapeData.Shape[,,] correctAnswer;
    /// <summary>
    /// 正しい答え
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer
    {
        get
        {
            return correctAnswer;
        }
        private set
        {
            correctAnswer = value;
        }
    }

    private ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// プレイヤーの答え
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer
    {
        get
        {
            return playerAnswer;
        }
        set
        {
            playerAnswer = value;
        }
    }

    private bool isClear = false;
    /// <summary>
    /// trueでステージクリア
    /// </summary>
    public bool IsClear
    {
        get
        {
            return isClear;
        }
        set
        {
            isClear = value;
        }
    }

    private bool isRetry = false;
    /// <summary>
    /// trueのときリトライ
    /// </summary>
    public bool IsRetry
    {
        get
        {
            return isRetry;
        }
        set
        {
            isRetry = value;
        }
    }

    private bool isPause = false;
    /// <summary>
    /// ポーズ状態かどうか
    /// </summary>
    public bool IsPause
    {
        get
        {
            return isPause;
        }
        set
        {
            isPause = value;
        }
    }

    private void Awake()
    {
        timeManager.OnStop();

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // 選択ステージ名を取得

        dataGot = false;
        stageDataLoader.StageDataGet(stageName);  // ステージの配置データをロード開始

        canToCheckPhase = true;
        confirmWindow.SetActive(false);

        // チュートリアルなら
        if(stageName == "Tutorial")
        {
            isTutorial = true;
            tutorial.TutorialStart();
        }

        loadManager = FindObjectOfType<LoadManager>();
        if (loadManager != null)
        {
            // フェード終了後スタート
            StartCoroutine(DelayCoroutine(() => { return loadManager.DidFadeComplete; },
                () => { timeManager.OnStart(); }));
        }
        else timeManager.OnStart();
    }

    void Update()
    {
        // ロードが終わっていなければ以降の処理に進ませない
        if (!stageDataLoader.stageDataLoadComlete) return;

        if (phase != PHASE.PLAY && tutorial.TutorialCompleteByPhase) isPause = !timeManager.TimeActive;
        else isPause = false;

        // データを変数として取得していなければ取得・初期化
        if (!dataGot) Initialize();

        if ((loadManager != null && loadManager.DidFadeComplete) || loadManager == null)
        {
            MainGameManage();
            ClearOrRetry();
        }
    }

    /// <summary>
    /// 変数/各フェーズの初期化　確認フェーズへの移行
    /// </summary>
    private void Initialize()
    {
        // マップサイズ取得
        MapSize = stageDataLoader.LoadStageSize();

        // 配列 要素数指定
        ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
        CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
        PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
        playerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

        // 配列の初期化
        for (int z = 0; z < MapSize.z; z++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                for (int x = 0; x < MapSize.x; x++)
                {
                    CorrectAnswer[x, y, z] = ShapeData.Shape.Empty;
                    PlayerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
        for (int i = 0; i < ShapeType.Length; i++)
        {
            ShapeType[i] = ShapeData.Shape.Empty;
        }

        // 正答取得
        CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

        // 使用している図形の種類を取得
        // この配列内にある図形だけ、選択フェーズのタブ内にボタンを表示する
        ShapeType = shapeData.ShapeTypes(CorrectAnswer);

        // 各クラスの初期化
        cameraRotate.Initialize();
        checkPhase.Initialize();
        selectPhase.Initialize();
        playPhase.Initialize();

        // シート作成
        sheatCreate.Create();

        // 確認フェーズに移行
        ToCheckPhase();

        dataGot = true;
    }

    /// <summary>
    /// フェーズ管理
    /// </summary>
    private void MainGameManage()
    {
        if (IsTutorial) tutorial.TutorialUpdate();

        switch (phase)
        {
            case PHASE.CHECK:
                cameraRotate.CameraUpdate();
                break;
            
            case PHASE.SELECT:
                selectPhase.PhaseUpdate();
                break;

            case PHASE.PLAY:
                playPhase.PhaseUpdate();
                break;
        }
    }

    /// <summary>
    /// クリア/リトライ時の処理
    /// </summary>
    private void ClearOrRetry()
    {
        if((isTutorial && !tutorial.IsTutorialComplete) || isPause) return;

        // クリア時の遷移処理
        if (IsClear && Input.touchCount >= 1)
        {
            // ステージ選択画面に戻る
            SceneLoader.Load("SelectScene");

            IsClear = false;
        }

        // 再挑戦時の処理
        if (IsRetry && Input.touchCount >= 1)
        {
            // 確認フェーズに戻る
            testButton.BackToggle();
            IsRetry = false;
        }
    }

    /// <summary>
    /// 確認ウィンドウの表示
    /// </summary>
    public void ConfirmDisp()
    {
        // 選択フェーズから確認フェーズに戻るボタンを押したときに呼ばれる
        if (phase == PHASE.SELECT)
        {
            numberOfReconfirmation.text = "現在の回数：" + Reconfirmation.ToString() + "回";
            confirmWindow.SetActive(true);
            timeManager.OnStop();
        }
    }

    /// <summary>
    ///  選択フェーズから確認フェーズへ移行
    /// </summary>
    /// <param name="_canToCheckPhase">確認フェーズに移行するか</param>
    public void Confirm(bool _canToCheckPhase)
    {
        // 確認ウィンドウのはい/いいえのボタンを押したときに呼ばれる
        if (phase == PHASE.SELECT)
        {
            canToCheckPhase = _canToCheckPhase;
            confirmWindow.SetActive(false);

            // 確認フェーズに戻るとき（『はい』を押されたとき）
            if (canToCheckPhase)
            {
                // フェーズ移行
                selectPhase.PhaseEnd();
                ToCheckPhase();

                Reconfirmation++;

                testButton.BackToggle();
                timeManager.OnStart();
            }
        }
    }

    /// <summary>
    /// 確認フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToCheckPhase()
    {
        switch (phase)
        {
            case PHASE.SELECT:
                break;
            case PHASE.PLAY:
                playPhase.PhaseEnd();
                Miss++;
                cameraRotate.FromPlayPhase();
                break;
        }

        if (canToCheckPhase)
        {
            phase = PHASE.CHECK;

            // 設定ボタン
            optionButton.SetActive(true);

            // タップ状態のリセット
            cameraRotate.TapReset();

            // シートの表示設定
            sheatCreate.SheatDisp(true, true);

            // 確認フェーズ開始処理
            checkPhase.PhaseStart();

            canToCheckPhase = false;
        }
    }

    /// <summary>
    /// 選択フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToSelectPhase()
    {
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.PhaseEnd();
                break;
            case PHASE.PLAY:
                playPhase.PhaseEnd();
                break;
        }

        phase = PHASE.SELECT;

        if(isTutorial) tutorial.ToSelectA = true;

        canToCheckPhase = false;

        // 設定ボタン
        optionButton.SetActive(true);

        // シート
        sheatCreate.SheatDisp(false, false);

        // 選択フェーズ開始処理
        selectPhase.PhaseStart();
    }

    /// <summary>
    /// 実行フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToPlayPhase()
    {
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.PhaseEnd();
                break;
            case PHASE.SELECT:
                selectPhase.PhaseEnd();
                break;
        }

        phase = PHASE.PLAY;

        if (isTutorial) tutorial.ToPlayA();

        canToCheckPhase = true;

        // 設定ボタン非表示
        optionButton.SetActive(false);

        // シート
        sheatCreate.SheatDisp(true, false);

        // カメラ 位置変更
        cameraRotate.ToPlayPhase();

        // 実行フェーズ開始処理
        playPhase.PhaseStart();
    }
}
