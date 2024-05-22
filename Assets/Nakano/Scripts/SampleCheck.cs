using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// サンプル生成確認用
/// </summary>
public class SampleCheck : MonoBehaviour
{
    [SerializeField, Header("ステージ名")] string stageName;

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    // サンプルの親オブジェクト
    [SerializeField] Transform objParent;

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // データ取得完了したかどうか
    bool dataGot = false;

    // 配置データ
    public ShapeData.Shape[,,] correctAnswer;

    // サンプル生成済みかどうか
    bool sampleCreated = false;

    void Awake()
    {
        stageDataLoader.StageDataGet(stageName);  // ステージの配置データをロード開始
    }

    void Update()
    {
        // ロードが終わっていなければ次の処理に進ませない
        if (!stageDataLoader.stageDataLoadComlete) return;

        // データを変数として取得していなければ
        if (!dataGot)
        {
            dataGot = true;

            // マップサイズ取得
            MapSize = stageDataLoader.LoadStageSize();

            // 配列 要素数指定
            correctAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            cameraRotate.TargetSet();
            cameraRotate.CanRotate = true;

            // 正答取得
            correctAnswer = stageDataLoader.LoadStageMap(MapSize);

            // シート作成
            sheatCreate.Sheat();

            StageInstance();
        }
    }
    void StageInstance()
    {
        // 生成済みなら再度生成しない
        if (sampleCreated) return;

        // 生成
        for (int z = 0; z < MapSize.z; z++)
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                for (int y = 0; y < MapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = correctAnswer[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    Instantiate(obj, pos, Quaternion.identity, objParent);
                }
            }
        }

        sampleCreated = true;
    }

}
