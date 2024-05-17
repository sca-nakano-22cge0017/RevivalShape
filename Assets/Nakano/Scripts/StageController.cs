using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageController : MonoBehaviour
{
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

    private int yDataMax = 10; //選択フェーズで1マス内に入力できる最大値

    Vector3 mapSize = new Vector3(4, 4, 4);
    public Vector3 MapSize { get { return mapSize; } }
    
    private int phaseBackCount = 0;
    /// <summary>
    /// 確認フェーズに戻った回数
    /// </summary>
    public int PhaseBackCount { get { return phaseBackCount; } set { phaseBackCount = value; } }

    ShapeData.Shape[] shapeType; // 使用図形の種類
    private int shapeTypeAmount = 0; // 使用図形の種類数
    public int ShapeTypeAmount { get { return shapeTypeAmount; } }

    ShapeData.Shape[,,] correctAnswer;
    /// <summary>
    /// 正しい答え
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer
    {
        get { return correctAnswer; }
        set { correctAnswer = value; }
    }

    ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// プレイヤーの答え
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer
    {
        get { return playerAnswer; }
        set { playerAnswer = value; }
    }

    // データ取得完了したかどうか
    bool mapSizeDataGot = false;
    bool stageDataGot = false;

    bool isClear = false;
    /// <summary>
    /// trueでステージクリア
    /// </summary>
    public bool IsClear { get { return isClear; } set{ isClear = value; } }

    /// <summary>
    /// trueのときリトライ
    /// </summary>
    bool isRetry = false;
    public bool IsRetry { get { return isRetry; } set { isRetry = value; } }

    private void Awake()
    {
        if(SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // 選択ステージ名を取得

        stageDataLoader.StageDataGet(stageName);
    }

    void Update()
    {
        // データのロードが完了していたら かつ データを変数として取得していなければ
        if(stageDataLoader.stageDataLoadComlete && !mapSizeDataGot)
        {
            // マップサイズ取得
            mapSize = stageDataLoader.LoadStageSize();
            shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;
            mapSize.y = yDataMax * shapeTypeAmount; // 図形の種類数に応じてプレイヤーの解答用の配列のサイズを変更する

            // 配列 要素数指定
            shapeType = new ShapeData.Shape[shapeTypeAmount];
            correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
            playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

            // 配置データロード
            cameraRotate.MapSizeInitialize();

            mapSizeDataGot = true;
        }

        // 正解の配置データを配列に入れる
        if (stageDataLoader.stageDataLoadComlete && !stageDataGot)
        {
            correctAnswer = stageDataLoader.LoadStageMap(mapSize);

            shapeType = shapeData.ShapeTypes(correctAnswer); // 使用している図形の種類を取得
            shapeTypeAmount = shapeData.ShapeTypesAmount(shapeType); // 使用している図形の種類数を取得

            sheatCreate.Sheat(); // シート作成
            ToCheckPhase(); // 確認フェーズに移行

            stageDataGot = true;
        }

        if (isClear && Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("SelectScene");
            isClear = false;
        }

        if (isRetry && Input.GetMouseButton(0))
        {
            // 確認フェーズに戻る
            ToCheckPhase();
            isRetry = false;
        }
    }

    /// <summary>
    /// 確認フェーズに移行
    /// </summary>
    public void ToCheckPhase()
    {
        phase = PHASE.CHECK;

        playPhase.PlayPhaseEnd();
        selectPhase.SelectPhaseEnd();

        cameraRotate.CanRotate = true; // カメラの回転ができるようにする

        sheatCreate.SheatDisp(true, true); // シートの表示設定

        checkPhase.CheckPhaseStart(); // 確認フェーズに移行する
    }

    /// <summary>
    /// 選択フェーズに移行
    /// </summary>
    public void ToSelectPhase()
    {
        phase = PHASE.SELECT;

        checkPhase.CheckPhaseEnd();
        playPhase.PlayPhaseEnd();

        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

        sheatCreate.SheatDisp(false, false);

        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// 実行フェーズに移行
    /// </summary>
    public void ToPlayPhase()
    {
        phase = PHASE.PLAY;

        checkPhase.CheckPhaseEnd();
        selectPhase.SelectPhaseEnd();

        sheatCreate.SheatDisp(true, false);
        
        playPhase.PlayPhaseStart();
    }
}
