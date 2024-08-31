using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombineTest : MonoBehaviour
{
    public MB3_MeshBaker meshbaker;
    public GameObject mCombineObj;
    private GameObject[] mObjArray;
    public MB2_TextureBakeResults texture;

    private void Start()
    {
        meshbaker.textureBakeResults = texture;

        meshbaker = GetComponent<MB3_MeshBaker>();

        // メッシュを結合するゲームオブジェクトを取得
        int length = mCombineObj.transform.childCount;
        mObjArray = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            mObjArray[i] = mCombineObj.transform.GetChild(i).gameObject;
        }

        // MeshBakerに登録
        meshbaker.AddDeleteGameObjects(mObjArray, null, false);

        // 登録されているゲームオブジェクトのメッシュを結合し、シーン内に出力
        meshbaker.Apply();

        // 結合元となるゲームオブジェクトを削除する
        Destroy(mCombineObj);
    }
}
