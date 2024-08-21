using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メッシュ結合
/// </summary>
public class MeshCombiner : MonoBehaviour
{
    private Transform fieldParent;

    // 結合したメッシュのマテリアル
    [SerializeField] private Material combinedMat;

    [SerializeField] private float outlineWidth = 5.0f;

    public void SetParent(Transform _parent)
    {
        fieldParent = _parent;
    }

    public void Combine()
    {
        CombineMesh();
    }

    /// <Summary>
    /// メッシュを結合
    /// </Summary>
    void CombineMesh()
    {
        MeshFilter parentMeshFilter = CheckParentComponent<MeshFilter>(fieldParent.gameObject);
        MeshRenderer parentMeshRenderer = CheckParentComponent<MeshRenderer>(fieldParent.gameObject);

        MeshFilter[] meshFilters = fieldParent.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilterList = new List<MeshFilter>();
        for (int i = 1; i < meshFilters.Length; i++)
        {
            meshFilterList.Add(meshFilters[i]);
        }

        // 結合するメッシュの配列を作成
        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        // 結合するメッシュの情報をCombineInstanceに追加
        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;
            //meshFilterList[i].gameObject.SetActive(false);
        }

        // 結合したメッシュをセット
        parentMeshFilter.mesh = new Mesh();
        parentMeshFilter.mesh.CombineMeshes(combine);

        // 結合したメッシュにマテリアルをセット
        parentMeshRenderer.material = combinedMat;

        // 親オブジェクトを表示
        fieldParent.gameObject.SetActive(true);

        // アウトライン描画
        DispOutline();
    }

    void DispOutline()
    {
        fieldParent.gameObject.AddComponent<Outline>();
        fieldParent.gameObject.GetComponent<Outline>().OutlineMode = Outline.Mode.OutlineVisible;
        fieldParent.gameObject.GetComponent<Outline>().OutlineColor = Color.black;
        fieldParent.gameObject.GetComponent<Outline>().OutlineWidth = outlineWidth;
    }

    /// <Summary>
    /// 指定されたコンポーネントへの参照を取得
    /// コンポーネントがない場合はアタッチ
    /// </Summary>
    T CheckParentComponent<T>(GameObject obj) where T : Component
    {
        var targetComp = obj.GetComponent<T>();
        if (targetComp == null)
        {
            targetComp = obj.AddComponent<T>();
        }
        return targetComp;
    }

    public void Remove()
    {
        RemoveMeshes(fieldParent.gameObject);
    }

    /// <Summary>
    /// 引数のオブジェクトのコンポーネントをデタッチします。
    /// </Summary>
    void RemoveMeshes(GameObject obj)
    {
        foreach (Component comp in obj.GetComponents<Component>())
        {
            if (comp.GetType() != typeof(Transform))
            {
                Destroy(comp);
            }
        }
    }
}
