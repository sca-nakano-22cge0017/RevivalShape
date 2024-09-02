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

    [SerializeField, Header("1から生成するか")] private bool isCreate = false;

    [SerializeField, Header("生成場所")] private Transform createParent;
    [SerializeField, Header("サンプルの親オブジェクト")] private Transform sampleParent;
    [SerializeField, Header("各ステージのサンプル")] private GameObject[] samples;
    [SerializeField, Header("各ステージのサンプル TutorialとExtra")] private GameObject[] samplesOther;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // サンプルのGameObject型配列

    private bool sampleCreated = false; // サンプル生成済みかどうか

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        sampleParent.gameObject.SetActive(false);

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
        sampleParent.gameObject.SetActive(true);

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
        sampleParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// サンプル生成
    /// </summary>
    private void SampleInstance()
    {
        if(!isCreate)
        {
            SampleDisplay();
            return;
        }

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
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, createParent);
                    }
                }
            }
        }

        sampleCreated = true;
    }

    /// <summary>
    /// 生成・結合済みサンプルを表示
    /// </summary>
    private void SampleDisplay()
    {
        string stageName = stageController.StageName;

        if (stageName.Contains("Stage"))
        {
            string _stageName = stageName.Replace("Stage", "");

            if (int.TryParse(_stageName, out int n))
            {
                if (n - 1 >= 0 && n - 1 < samples.Length)
                {
                    if (!SampleNullCheck(samples[n - 1])) samples[n - 1].SetActive(true);
                }

                return;
            }
        }

        else if (stageName.Contains("Tutorial"))
        {
            if (!SampleNullCheck(samplesOther[0])) samplesOther[0].SetActive(true);
        }

        else if (stageName.Contains("Extra"))
        {
            string _stageName = stageName.Replace("Extra", "");

            if (int.TryParse(_stageName, out int n))
            {
                if (!SampleNullCheck(samplesOther[n])) samplesOther[n].SetActive(true);
            }
        }
    }

    bool SampleNullCheck(GameObject _object)
    {
        bool isNull = false;

        if (_object == null)
        {
            isNull = true;
            isCreate = true;
            SampleInstance();
        }

        return isNull;
    }
}
