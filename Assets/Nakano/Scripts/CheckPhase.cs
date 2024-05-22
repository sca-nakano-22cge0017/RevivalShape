using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 確認フェーズ
/// </summary>
public class CheckPhase : MonoBehaviour
{
    [SerializeField] ShapeData shapeData;
    [SerializeField] StageController stageController;

    // サンプルの親オブジェクト
    [SerializeField] Transform objParent;

    [SerializeField] GameObject checkPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map; // 配置データ
    GameObject[,,] mapObj;   // サンプルのGameObject型配列

    // サンプル生成済みかどうか
    bool sampleCreated = false;

    private void Awake()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// 確認フェーズ移行時の処理
    /// UI表示、サンプル生成
    /// </summary>
    public void CheckPhaseStart()
    {
        checkPhaseUI.SetActive(true);
        objParent.gameObject.SetActive(true);

        // オブジェクト生成
         StageInstance();
    }

    /// <summary>
    /// 確認フェーズ終了
    /// UI非表示
    /// </summary>
    public void CheckPhaseEnd()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// サンプル生成
    /// </summary>
    void StageInstance()
    {
        // 生成済みなら再度生成しない
        if (sampleCreated) return;

        // サイズ代入
        mapSize = stageController.MapSize;

        // 配列 要素数指定
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // 正解の配置データを取得
        map = stageController.CorrectAnswer;

        // 生成
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
                    mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = false; // 振動オフ
                }
            }
        }

        sampleCreated = true;
    }
}
