using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField, Header("ステージ名")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;

    [SerializeField] CheckPhase checkPhase;
    [SerializeField] SelectPhase selectPhase;
    [SerializeField] PlayPhase playPhase;

    Vector3 mapSize = new Vector3(4, 4, 4);
    public Vector3 MapSize { get { return mapSize; } }

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

    bool mapSizeDataGot = false;
    bool stageDataGot = false;

    void Update()
    {
        // データのロードが完了していたら かつ データを変数として取得していなければ
        if(stageDataLoader.mapSizeDataLoadComlete && !mapSizeDataGot)
        {
            // マップサイズ取得
            mapSize = stageDataLoader.LoadStageSize(stageName);

            // 配列 要素数指定
            correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
            playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

            mapSizeDataGot = true;

            // 配置データロード
            stageDataLoader.StageDataGet(stageName);

            cameraRotate.MapSizeInitialize();
        }

        // 正解の配置データを配列に入れる
        if (stageDataLoader.stageDataLoadComlete && !stageDataGot)
        {
            correctAnswer = stageDataLoader.LoadStageMap(mapSize);

            stageDataGot = true;

            checkPhase.CheckPhaseStart(); // 確認フェーズに移行する
        }
    }

    /// <summary>
    /// 選択フェーズに移行
    /// </summary>
    public void ToSelectPhase()
    {
        checkPhase.CheckPhaseEnd();

        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// 実行フェーズに移行
    /// </summary>
    public void ToPlayPhase()
    {
        selectPhase.SelectPhaseEnd();

        StartCoroutine(PlayPhaseStart());
    }

    IEnumerator PlayPhaseStart()
    {
        yield return new WaitForSeconds(1f);

        playPhase.PlayPhaseStart();
    }
}
