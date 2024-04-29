using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPhase : MonoBehaviour
{
    string stageName;

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;
    [SerializeField] StageController stageController;

    [SerializeField] Transform objParent;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    void Start()
    {
        stageName = stageController.StageName; // ステージ名取得
        mapSize = stageController.MapSize; // サイズ代入

        // 配列初期化
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // 配置データ取得
        map = stageDataLoader.LoadStageMap(stageName);
        stageController.CorrectAnswer = map; // 正答として保存
        
        StageInstance();
    }

    void Update()
    {
        
    }

    void StageInstance()
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

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                }
            }
        }

        //StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    mapObj[x, y, z].SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }
}
