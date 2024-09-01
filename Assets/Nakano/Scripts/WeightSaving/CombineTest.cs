using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// メッシュ結合、テクスチャを設定する
/// DrawCallを減らすための機能 図形毎にスクリプトを用意する必要がある
/// </summary>
public class CombineTest : MonoBehaviour
{
    [SerializeField] private ShapeData shapeData;

    [SerializeField] private MB3_MeshBaker meshbaker;
    private GameObject[] mObjArray;
    private MB2_TextureBakeResults texture;
    bool isTextureLoaded = false;

    [SerializeField] private string fileFormat1 = "";
    [SerializeField] private string fileFormat2 = "";

    /// <summary>
    /// メッシュ結合・テクスチャの追加
    /// </summary>
    /// <param name="_stageName">ステージ名</param>
    /// <param name="_shape">図形</param>
    /// <param name="_parent">結合するブロックの親オブジェクト</param>
    public void Combine(string _stageName, ShapeData.Shape _shape, Transform _parent)
    {
        StartCoroutine(CombineCoroutine(_stageName, _shape, _parent));
    }

    /// <summary>
    /// テクスチャの取得 Addressableを使用
    /// </summary>
    /// <param name="_stageName"></param>
    /// <param name="_shape"></param>
    void GetTexture(string _stageName, ShapeData.Shape _shape)
    {
        string fileName = fileFormat1 + _stageName + "/" + _shape + fileFormat2;
        AsyncOperationHandle<MB2_TextureBakeResults> m_TextHandle;

        // マップサイズのデータを取得
        Addressables.LoadAssetAsync<MB2_TextureBakeResults>(fileName).Completed += handle => {
            m_TextHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }
            texture = handle.Result;
            isTextureLoaded = true;
        };
    }

    void CombineMesh(Transform _parent)
    {
        if (texture == null) return;

        meshbaker = GetComponent<MB3_MeshBaker>();
        meshbaker.textureBakeResults = texture;

        // メッシュを結合するゲームオブジェクトを取得
        int length = _parent.transform.childCount;
        mObjArray = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            mObjArray[i] = _parent.transform.GetChild(i).gameObject;
        }

        // MeshBakerに登録
        meshbaker.AddDeleteGameObjects(mObjArray, null, false);

        // 登録されているゲームオブジェクトのメッシュを結合し、シーン内に出力
        meshbaker.Apply();

        // 結合元となるゲームオブジェクトを削除する
        _parent.gameObject.SetActive(false);
    }

    IEnumerator CombineCoroutine(string _stageName, ShapeData.Shape _shape, Transform _parent)
    {
        GetTexture(_stageName, _shape);

        yield return new WaitUntil(() => isTextureLoaded);

        CombineMesh(_parent);
    }
}
