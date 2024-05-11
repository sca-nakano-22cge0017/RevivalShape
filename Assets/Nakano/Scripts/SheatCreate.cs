using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheatCreate : MonoBehaviour
{
    [SerializeField] GameObject sheatPrefab;
    [SerializeField] Transform checkPhaseParent;
    [SerializeField] Transform playPhaseParent;

    [SerializeField] StageController stageController;

    Vector3 mapSize;

    public void Sheat()
    {
        mapSize = stageController.MapSize; // サイズ代入

        // マップの広さ分シートをを生成
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                //Instantiate(sheatPrefab, new Vector3(x, -0.5f, z))
            }
        }
    }
}
