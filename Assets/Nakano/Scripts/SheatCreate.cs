using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シート作成
/// </summary>
public class SheatCreate : MonoBehaviour
{
    [SerializeField] private GameObject sheatPrefab;
    [SerializeField] private Transform sheatParent;
    [SerializeField] private GameObject marks;

    [SerializeField] private GameObject frontMark;
    [SerializeField] private GameObject backMark;

    [SerializeField] private StageController stageController;

    private Vector3 mapSize;

    private bool isCreated = false;

    /// <summary>
    /// シート作成
    /// </summary>
    public void Sheat()
    {
        if(isCreated) return;

        // サイズ代入
        if (stageController)
            mapSize = stageController.MapSize;

        // マップの広さ分シートをを生成
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                Instantiate(sheatPrefab, new Vector3(-x, -0.5f, z), Quaternion.identity, sheatParent);
            }
        }

        frontMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, mapSize.z + 1.5f);
        backMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, -2.5f);

        isCreated = true;
    }

    /// <summary>
    /// シート、マークの表示・非表示
    /// </summary>
    /// <param name="isDisp">trueのときシート表示</param>
    /// <param name="isMarkDisp">trueのときマーク表示</param>
    public void SheatDisp(bool isDisp, bool isMarkDisp)
    {
        sheatParent.gameObject.SetActive(isDisp);
        marks.SetActive(isMarkDisp);
    }
}
