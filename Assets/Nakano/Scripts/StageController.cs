using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// メインゲーム制御
/// </summary>
public class StageController : MonoBehaviour
{
    // フェーズ
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = 0;

    [SerializeField, Header("ステージ名")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    [SerializeField] CheckPhase checkPhase;
    [SerializeField] SelectPhase selectPhase;
    [SerializeField] PlayPhase playPhase;

    //選択フェーズで1マス内に入力できる最大値
    private int yDataMax = 10;

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // データ取得完了したかどうか
    bool dataGot = false;

    /// <summary>
    /// 確認フェーズに戻った回数
    /// </summary>
    public int PhaseBackCount { get; set; } = 0;

    /// <summary>
    /// 使用図形の種類一覧
    /// </summary>
    public ShapeData.Shape[] ShapeType { get; private set; }

    /// <summary>
    /// 使用図形の種類数
    /// </summary>
    public int ShapeTypeAmount { get; private set; } = 0;

    /// <summary>
    /// 正しい答え
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer { get; set; }

    /// <summary>
    /// プレイヤーの答え
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer { get; set; }

    /// <summary>
    /// trueでステージクリア
    /// </summary>
    public bool IsClear { get; set; } = false;

    /// <summary>
    /// trueのときリトライ
    /// </summary>
    public bool IsRetry { get; set; } = false;

    private void Awake()
    {
        if(SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // 選択ステージ名を取得

        stageDataLoader.StageDataGet(stageName);  // ステージの配置データをロード開始
    }

    void Update()
    {
        // ロードが終わっていなければ次の処理に進ませない
        if (!stageDataLoader.stageDataLoadComlete) return;

        // データを変数として取得していなければ
        if(!dataGot)
        {
            dataGot = true;

            // マップサイズ取得
            MapSize = stageDataLoader.LoadStageSize();
            
            // ゲームに登場する図形の種類数を取得
            ShapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

            // 図形の種類数に応じてプレイヤーの解答用の配列のサイズを変更する
            MapSize = new Vector3(MapSize.x, yDataMax * ShapeTypeAmount, MapSize.z);

            // 配列 要素数指定
            ShapeType = new ShapeData.Shape[ShapeTypeAmount];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            cameraRotate.TargetSet();

            // 正答取得
            CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

            // 使用している図形の種類を取得
            ShapeType = shapeData.ShapeTypes(CorrectAnswer);

            // 使用している図形の種類数を取得
            ShapeTypeAmount = shapeData.ShapeTypesAmount(ShapeType);

            // シート作成
            sheatCreate.Sheat();

            // 確認フェーズに移行
            ToCheckPhase();
        }

        // クリア時の遷移処理
        if (IsClear && Input.GetMouseButton(0))
        {
            // ステージ選択画面に戻る
            SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // 再挑戦時の処理
        if (IsRetry && Input.GetMouseButton(0))
        {
            // 確認フェーズに戻る
            ToCheckPhase();
            IsRetry = false;
        }
    }

    /// <summary>
    /// 確認フェーズに移行
    /// フェーズを管理するenum型変数の変更、他フェーズ用のUI非表示等
    /// </summary>
    public void ToCheckPhase()
    {
        phase = PHASE.CHECK;

        playPhase.PlayPhaseEnd();
        selectPhase.SelectPhaseEnd();

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
        phase = PHASE.SELECT;

        checkPhase.CheckPhaseEnd();
        playPhase.PlayPhaseEnd();

        // カメラ
        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

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
        phase = PHASE.PLAY;

        checkPhase.CheckPhaseEnd();
        selectPhase.SelectPhaseEnd();

        // シート
        sheatCreate.SheatDisp(true, false);

        // 実行フェーズ開始処理
        playPhase.PlayPhaseStart();
    }
}
