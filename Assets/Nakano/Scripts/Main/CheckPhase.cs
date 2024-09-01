using UnityEngine;

[System.Serializable]
public class CombineTests
{
    public ShapeData.Shape shape;
    public CombineTest combineTest;
    public Transform parent;        // parentの子オブジェクトを全て結合する
    public GameObject combinedObj;  // 結合後のオブジェクト
}

/// <summary>
/// 確認フェーズ
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;
    [SerializeField] private MeshCombiner meshCombiner;
    [SerializeField] private CombineTests[] combineTests;
    [SerializeField, Header("結合するか")] private bool isCombine = false;

    // サンプルの親オブジェクト
    [SerializeField] private Transform objParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // サンプルのGameObject型配列

    private bool sampleCreated = false; // サンプル生成済みかどうか

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);

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
        objParent.gameObject.SetActive(true);

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

                    // 空白マスは生成しない
                    if (s != ShapeData.Shape.Empty)
                    {
                        Transform parent = GetParent(s);
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, parent);
                    }
                }
            }
        }

        sampleCreated = true;

        if(isCombine) Combine();
    }

    // メッシュ結合
    void Combine()
    {
        for (int i = 0; i < combineTests.Length; i++)
        {
            if (combineTests[i].shape != ShapeData.Shape.Empty)
            {
                Transform parent = combineTests[i].parent;
                CombineTest ct = combineTests[i].combineTest;
                ct.Combine(stageController.StageName, combineTests[i].shape, parent);
            }
        }
    }

    Transform GetParent(ShapeData.Shape _shape)
    {
        Transform objParent = null;

        for (int i = 0; i < combineTests.Length; i++)
        {
            if (combineTests[i].shape == _shape)
            {
                objParent = combineTests[i].parent;
            }
        }

        return objParent;
    }

    /// <summary>
    /// メッシュ結合後のオブジェクトを取得
    /// </summary>
    void GetCombinedObject()
    {

    }
}
