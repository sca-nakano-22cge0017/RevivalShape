using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���b�V������
/// </summary>
public class MeshCombiner : MonoBehaviour
{
    private Transform fieldParent;

    // �����������b�V���̃}�e���A��
    [SerializeField] private Material combinedMat;

    public void SetParent(Transform _parent)
    {
        fieldParent = _parent;
    }

    /// <summary>
    /// ���b�V������
    /// </summary>
    /// <param name="_childrenDisplay">���������I�u�W�F�N�g��\�����邩</param>
    public void Combine(bool _isChildrenDisplay, bool _isMeshRenderer)
    {
        CombineMesh(_isChildrenDisplay,_isMeshRenderer);
    }

    void CombineMesh(bool _isChildrenDisplay, bool _isMeshRenderer)
    {
        MeshFilter parentMeshFilter = CheckParentComponent<MeshFilter>(fieldParent.gameObject);
        MeshRenderer parentMeshRenderer = CheckParentComponent<MeshRenderer>(fieldParent.gameObject);

        MeshFilter[] meshFilters = fieldParent.GetComponentsInChildren<MeshFilter>();

        // ���b�V�������������ΏI��
        if(meshFilters.Length <= 0) return;

        List<MeshFilter> meshFilterList = new List<MeshFilter>();

        for (int i = 1; i < meshFilters.Length; i++)
        {
            meshFilterList.Add(meshFilters[i]);
        }

        // �������郁�b�V���̔z����쐬
        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        // �������郁�b�V���̏���CombineInstance�ɒǉ�
        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;

            meshFilterList[i].gameObject.SetActive(_isChildrenDisplay);
            meshFilterList[i].gameObject.GetComponent<MeshRenderer>().enabled = _isMeshRenderer;
        }

        // �����������b�V�����Z�b�g
        parentMeshFilter.mesh = new Mesh();
        parentMeshFilter.mesh.CombineMeshes(combine);

        // �����������b�V���Ƀ}�e���A�����Z�b�g
        parentMeshRenderer.material = combinedMat;

        // �e�I�u�W�F�N�g��\��
        fieldParent.gameObject.SetActive(true);
    }


    /// <Summary>
    /// �w�肳�ꂽ�R���|�[�l���g�ւ̎Q�Ƃ��擾
    /// �R���|�[�l���g���Ȃ��ꍇ�̓A�^�b�`
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
    /// �����̃I�u�W�F�N�g�̃R���|�[�l���g���f�^�b�`���܂��B
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
