using UnityEngine;

/// <summary>
/// 確認フェーズ
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;

    [SerializeField] private Combiners[] combiners;

    [SerializeField, Header("生成場所")] private Transform samplesParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // サンプルのGameObject型配列

    private bool sampleCreated = false; // サンプル生成済みかどうか

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        samplesParent.gameObject.SetActive(false);

        // マップサイズ取得
        mapSize = stageController.MapSize;

        // 配列 要素数指定
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
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
        samplesParent.gameObject.SetActive(true);

        UsedBlocksCheck();

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
        samplesParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// サンプル生成
    /// </summary>
    private void SampleInstance()
    {
        // 生成済みなら再度生成しない
        if (sampleCreated) return;

        // オブジェクト生成
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = stageController.CorrectAnswer[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);
                    Transform parent = GetCreateParent(s);
                    
                    // 空白マスは生成しない
                    if (s != ShapeData.Shape.Empty)
                    {
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, parent);
                    }
                }
            }
        }

        sampleCreated = true;

        MeshCombine();
    }

    void UsedBlocksCheck()
    {
        for (int s = 0; s < stageController.ShapeType.Length; s++)
        {
            for (int c = 0; c < combiners.Length; c++)
            {
                if(combiners[c].shape == stageController.ShapeType[s])
                {
                    combiners[c].isShapeUsed = true;
                }
            }
        }
    }

    void MeshCombine()
    {
        for (int i = 0; i < combiners.Length; i++)
        {
            if (combiners[i].isShapeUsed)
            {
                combiners[i].meshCombiner.SetParent(combiners[i].parent);
                combiners[i].meshCombiner.Combine(true, false);
            }
        }
    }

    /// <summary>
    /// 生成ブロックに応じて生成する親オブジェクトを返す
    /// </summary>
    /// <param name="_shape"></param>
    Transform GetCreateParent(ShapeData.Shape _shape)
    {
        Transform parent = null;

        for (int i = 0; i < combiners.Length; i++)
        {
            if (combiners[i].shape == _shape)
            {
                parent = combiners[i].parent;
                break;
            }
        }

        return parent;
    }
}
