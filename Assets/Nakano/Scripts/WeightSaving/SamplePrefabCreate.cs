using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SamplePrefabCreate : MonoBehaviour
{
    [SerializeField, Header("結合するか"), 
        Tooltip("textureを結合する場合はfalseにしてオブジェクトを生成→PlayModeSaverで保存→TextureBakerでテクスチャ結合")] 
    private bool isCombine = false;

    [SerializeField] private string stageName;
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private Transform parent_Cube;
    [SerializeField] private Transform parent_Sphere;
    [SerializeField] private Transform parent_Alpha;

    [SerializeField] private CombineTest combineTest_Cube;
    [SerializeField] private CombineTest combineTest_Sphere;
    [SerializeField] private CombineTest combineTest_Alpha;

    ShapeData.Shape[] shapeType;

    private Vector3 mapSize;
    private ShapeData.Shape[,,] map; // 配置データ

    bool dataGot = false;
    bool created = false;
    bool combined = false;

    void Awake()
    {
        stageDataLoader.StageDataGet(stageName);
    }

    void Update()
    {
        if (!stageDataLoader.stageDataLoadComlete) return;

        if (!dataGot) Initialize();
        if (dataGot && !created)  SampleCreate();
        if (isCombine && created && !combined) Combine();
    }

    void Initialize()
    {
        mapSize = stageDataLoader.LoadStageSize();

        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }

        map = stageDataLoader.LoadStageMap(mapSize);

        dataGot = true;
    }

    void SampleCreate()
    {
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
                    if (s != ShapeData.Shape.Empty)
                    {
                        Transform objParent = GetParent(s);

                        Instantiate(obj, pos, Quaternion.identity, objParent);
                    }
                }
            }
        }

        created = true;
    }

    void Combine()
    {
        shapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
        shapeType = shapeData.ShapeTypes(map);

        for (int i = 0; i < shapeType.Length; i++)
        {
            if (shapeType[i] != ShapeData.Shape.Empty)
            {
                Transform parent = GetParent(shapeType[i]);
                CombineTest ct = GetCombineTest(shapeType[i]);
                ct.Combine(stageName, shapeType[i], parent);
            }
        }
    }

    Transform GetParent(ShapeData.Shape _shape)
    {
        Transform objParent = null;

        if (_shape == ShapeData.Shape.Cube) objParent = parent_Cube;
        if (_shape == ShapeData.Shape.Sphere) objParent = parent_Sphere;
        if (_shape == ShapeData.Shape.Alpha) objParent = parent_Alpha;

        return objParent;
    }

    CombineTest GetCombineTest(ShapeData.Shape _shape)
    {
        CombineTest ct = null;

        if (_shape == ShapeData.Shape.Cube) ct = combineTest_Cube;
        if (_shape == ShapeData.Shape.Sphere) ct = combineTest_Sphere;
        if (_shape == ShapeData.Shape.Alpha) ct = combineTest_Alpha;

        return ct;
    }
}
