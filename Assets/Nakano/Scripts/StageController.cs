using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private MissionScore missionScore;

    [SerializeField] private CameraRotate cameraRotate;
    [SerializeField] private SheatCreate sheatCreate;

    [SerializeField] private CheckPhase checkPhase;
    [SerializeField] private SelectPhase selectPhase;
    [SerializeField] private PlayPhase playPhase;

    [SerializeField] private TestButton testButton;

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
    public ShapeData.Shape[,,] CorrectAnswer { get { return correctAnswer; } set { correctAnswer = value; } }

    private ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// プレイヤーの答え
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer { get { return playerAnswer; } set { playerAnswer = value; } }

    private bool isClear = false;
    /// <summary>
    /// trueでステージクリア
    /// </summary>
    public bool IsClear { get{ return isClear; } set{ isClear = value;} }

    private bool isRetry = false;
    /// <summary>
    /// trueのときリトライ
    /// </summary>
    public bool IsRetry { get{ return isRetry; } set{ isRetry = value; } }

    private bool isStop = false;
    public bool IsStop { get { return isStop; } set { isStop = value; } }

    [Header("図形描画")]
    [SerializeField, Header("描画範囲 左上")] private Vector2 drawRangeMin = new Vector2(0, 370);
    [SerializeField, Header("描画範囲 右下")] private Vector2 drawRangeMax = new Vector2(1080, 1700);
    [SerializeField] private Texture _texture;
    [SerializeField] private bool isDragRangeDraw = false;

    private void Awake()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        _texture = texture;

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // 選択ステージ名を取得

        dataGot = false;
        stageDataLoader.StageDataGet(stageName);  // ステージの配置データをロード開始

        SceneController.SetCurrentSceneName();
    }

    void Update()
    {
        // ロードが終わっていなければ次の処理に進ませない
        if (!stageDataLoader.stageDataLoadComlete) return;

        // データを変数として取得していなければ
        if (!dataGot)
        {
            // マップサイズ取得
            MapSize = stageDataLoader.LoadStageSize();

            // 配列 要素数指定
            ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            playerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

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

            cameraRotate.CameraSetting();

            // 正答取得
            CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

            // 使用している図形の種類を取得
            ShapeType = shapeData.ShapeTypes(CorrectAnswer);

            checkPhase.Initialize();
            selectPhase.Initialize();
            playPhase.Initialize();

            // シート作成
            sheatCreate.Sheat();

            // 確認フェーズに移行
            ToCheckPhase();

            dataGot = true;
        }

        // クリア時の遷移処理
        if (IsClear && Input.touchCount >= 1 && !isStop)
        {
            // ステージ選択画面に戻る
            if (!playPhase.IsDebug)
                SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // 再挑戦時の処理
        if (IsRetry && Input.touchCount >= 1 && !isStop)
        {
            // 確認フェーズに戻る
            if (!playPhase.IsDebug)
            {
                testButton.BackToggle();
            }

            IsRetry = false;
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
                selectPhase.SelectPhaseEnd();
                Reconfirmation++;
                break;
            case PHASE.PLAY:
                playPhase.PlayPhaseEnd();
                Miss++;
                cameraRotate.Restore();
                break;
        }

        phase = PHASE.CHECK;

        // カメラの回転ができるようにする
        cameraRotate.CanRotate = true;

        // シートの表示設定
        sheatCreate.SheatDisp(true, true);

        // 確認フェーズ開始処理
        checkPhase.CheckPhaseStart();
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
                checkPhase.CheckPhaseEnd();
                break;
            case PHASE.PLAY:
                playPhase.PlayPhaseEnd();
                break;
        }

        phase = PHASE.SELECT;

        // カメラ
        cameraRotate.CanRotate = false;

        // シート
        sheatCreate.SheatDisp(false, false);

        // 選択フェーズ開始処理
        selectPhase.SelectPhaseStart();
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
                checkPhase.CheckPhaseEnd();
                break;
            case PHASE.SELECT:
                selectPhase.SelectPhaseEnd();
                break;
        }

        phase = PHASE.PLAY;

        // シート
        sheatCreate.SheatDisp(true, false);

        cameraRotate.PlayPhaseCamera();

        // 実行フェーズ開始処理
        playPhase.PlayPhaseStart();
    }

    public void Pause()
    {
        isStop = !isStop;
    }

    private void OnGUI()
    {
        if (isDragRangeDraw)
        {
            var rect = new Rect(drawRangeMin.x, drawRangeMin.y, drawRangeMax.x - drawRangeMin.x, drawRangeMax.y - drawRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }

    public IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }
}
