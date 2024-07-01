using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メッシュ結合
/// </summary>
public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private Transform fieldParent;

    // 結合したメッシュのマテリアル
    [SerializeField] private Material combinedMat;

    public void Combine()
    {
        CombineMesh();
    }

    /// <Summary>
    /// メッシュを結合します。
    /// </Summary>
    void CombineMesh()
    {
        // 親オブジェクトにMeshFilterがあるかどうか確認します。
        MeshFilter parentMeshFilter = CheckParentComponent<MeshFilter>(fieldParent.gameObject);

        // 親オブジェクトにMeshRendererがあるかどうか確認します。
        MeshRenderer parentMeshRenderer = CheckParentComponent<MeshRenderer>(fieldParent.gameObject);

        // 子オブジェクトのMeshFilterへの参照を配列として保持します。
        // ただし、親オブジェクトのメッシュもGetComponentsInChildrenに含まれるので除外します。
        MeshFilter[] meshFilters = fieldParent.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilterList = new List<MeshFilter>();
        for (int i = 1; i < meshFilters.Length; i++)
        {
            meshFilterList.Add(meshFilters[i]);
        }

        // 結合するメッシュの配列を作成します。
        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        // 結合するメッシュの情報をCombineInstanceに追加していきます。
        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;
            meshFilterList[i].gameObject.SetActive(false);
        }

        // 結合したメッシュをセットします。
        parentMeshFilter.mesh = new Mesh();
        parentMeshFilter.mesh.CombineMeshes(combine);

        // 結合したメッシュにマテリアルをセットします。
        parentMeshRenderer.material = combinedMat;

        // 親オブジェクトを表示します。
        fieldParent.gameObject.SetActive(true);
    }

    /// <Summary>
    /// 指定されたコンポーネントへの参照を取得します。
    /// コンポーネントがない場合はアタッチします。
    /// </Summary>
    T CheckParentComponent<T>(GameObject obj) where T : Component
    {
        // 型パラメータで指定したコンポーネントへの参照を取得します。
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
        // 親オブジェクトのコンポーネントを取得し、Transform以外のコンポーネントをデタッチします。
        foreach (Component comp in obj.GetComponents<Component>())
        {
            if (comp.GetType() != typeof(Transform))
            {
                Destroy(comp);
            }
        }
    }
}
