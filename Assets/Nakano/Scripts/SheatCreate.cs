using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheatCreate : MonoBehaviour
{
    [SerializeField] GameObject sheatPrefab;
    [SerializeField] Transform checkPhaseParent;
    [SerializeField] Transform playPhaseParent;

    [SerializeField] GameObject frontMark;
    [SerializeField] GameObject backMark;

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
                Instantiate(sheatPrefab, new Vector3(-x, -0.5f, z), Quaternion.identity, checkPhaseParent);
            }
        }

        frontMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, mapSize.z + 0.5f);
        backMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, -1.5f);
    }

    public void PlayPhase()
    {
        checkPhaseParent.SetParent(playPhaseParent);
    }
}
