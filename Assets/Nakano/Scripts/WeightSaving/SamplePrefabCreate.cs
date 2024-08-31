using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SamplePrefabCreate : MonoBehaviour
{
    [SerializeField] private string stageName;
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private Transform objParent;

    private Vector3 mapSize;
    private ShapeData.Shape[,,] map; // 配置データ

    bool dataGot = false;
    bool created = false;

    void Awake()
    {
        stageDataLoader.StageDataGet(stageName);
    }

    void Update()
    {
        if (!stageDataLoader.stageDataLoadComlete) return;

        if(!dataGot) Initialize();
        if(dataGot && !created)  SampleCreate();
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
                        Instantiate(obj, pos, Quaternion.identity, objParent);
                    }
                }
            }
        }

        created = true;
    }
}
