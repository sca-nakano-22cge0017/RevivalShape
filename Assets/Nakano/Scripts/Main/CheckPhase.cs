using UnityEngine;

/// <summary>
/// 確認フェーズ
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;
    [SerializeField] private MeshCombiner meshCombiner;

    // サンプルの親オブジェクト
    [SerializeField] private Transform objParent;
    [SerializeField] private Transform clearObjParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private ShapeData.Shape[,,] map; // 配置データ
    private GameObject[,,] mapObj;   // サンプルのGameObject型配列

    private bool sampleCreated = false; // サンプル生成済みかどうか

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列 要素数指定
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = ShapeData.Shape.Empty;
                    mapObj[x, y, z] = null;
                }
            }
        }
    }

    /// <summary>
    /// 確認フェーズ移行時の処理
    /// UI表示、サンプル生成
    /// </summary>
    public void PhaseStart()
    {
        checkPhaseUI.SetActive(true);
        objParent.gameObject.SetActive(true);
        clearObjParent.gameObject.SetActive(true);

        // オブジェクト生成
        SampleInstance();
    }

    public void PhaseUpdate()
    {

    }

    /// <summary>
    /// 確認フェーズ終了
    /// UI非表示
    /// </summary>
    public void PhaseEnd()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
        clearObjParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// サンプル生成
    /// </summary>
    private void SampleInstance()
    {
        // 生成済みなら再度生成しない
        if (sampleCreated) return;

        // 正解の配置データを取得
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = stageController.CorrectAnswer[x, y, z];
                }
            }
        }

        bool hasClearBlock = false; // 半透明ブロックがあるかどうか

        // オブジェクト生成
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    // 空白マスは生成しない
                    if (s != ShapeData.Shape.Empty && s != ShapeData.Shape.Alpha)
                    {
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = false; // 振動オフ
                    }

                    // 半透明ブロックは別オブジェクトの子として生成
                    if (s == ShapeData.Shape.Alpha)
                    {
                        hasClearBlock = true;

                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, clearObjParent);
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = false; // 振動オフ
                    }
                }
            }
        }

        sampleCreated = true;

        // 半透明ブロックのメッシュ結合
        if (hasClearBlock)
        {
            meshCombiner.SetParent(clearObjParent);
            meshCombiner.Combine(false);
        }
    }
}
