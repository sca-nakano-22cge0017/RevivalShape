using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SheatPrefabs
{
    public int height;
    public int width;
    public GameObject prefab;
}

/// <summary>
/// シート管理
/// </summary>
public class SheatCreate : MonoBehaviour
{
    [SerializeField] private StageController stageController;

    [SerializeField] private GameObject sheatPrefab;

    [SerializeField] private SheatPrefabs[] prefabs;

    [SerializeField] private Transform sheatParent;
    [SerializeField] private GameObject marks;

    [SerializeField, Header("『前』のマーク")] private GameObject frontMark;
    [SerializeField, Header("『後』のマーク")] private GameObject backMark;

    private Vector3 mapSize;

    private bool isCreated = false;
    private const float markPosY = -0.5f;

    [SerializeField, Header("生成位置 Y座標")] private float createPosY = -0.5f;

    /// <summary>
    /// シート作成
    /// </summary>
    public void Create()
    {
        // 再度生成しないようにする
        if(isCreated) return;

        // サイズ取得
        if (stageController)
            mapSize = stageController.MapSize;

        // マップの広さ分シートを生成
        //for (int x = 0; x < (int)mapSize.x; x++)
        //{
        //    for (int z = 0; z < (int)mapSize.z; z++)
        //    {
        //        Instantiate(sheatPrefab, new Vector3(-x, createPosY, z), Quaternion.identity, sheatParent);
        //    }
        //}

        // マップの広さ分シートを表示
        for (int i = 0; i < prefabs.Length; i++)
        {
            if ((int)mapSize.x == prefabs[i].width && (int)mapSize.z == prefabs[i].height)
            {
                prefabs[i].prefab.SetActive(true);
            }
        }

        // 前後を示すマークの位置を設定
        var markPosX = -mapSize.x / 2 + 0.5f;
        frontMark.transform.position = new Vector3(markPosX, markPosY, mapSize.z + 1.5f);
        backMark.transform.position  = new Vector3(markPosX, markPosY, -2.5f);

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
