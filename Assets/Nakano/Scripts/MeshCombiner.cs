using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���b�V������
/// </summary>
public class MeshCombiner : MonoBehaviour
{
    [SerializeField] private Transform fieldParent;

    // �����������b�V���̃}�e���A��
    [SerializeField] private Material combinedMat;

    public void Combine()
    {
        CombineMesh();
    }

    /// <Summary>
    /// ���b�V�����������܂��B
    /// </Summary>
    void CombineMesh()
    {
        // �e�I�u�W�F�N�g��MeshFilter�����邩�ǂ����m�F���܂��B
        MeshFilter parentMeshFilter = CheckParentComponent<MeshFilter>(fieldParent.gameObject);

        // �e�I�u�W�F�N�g��MeshRenderer�����邩�ǂ����m�F���܂��B
        MeshRenderer parentMeshRenderer = CheckParentComponent<MeshRenderer>(fieldParent.gameObject);

        // �q�I�u�W�F�N�g��MeshFilter�ւ̎Q�Ƃ�z��Ƃ��ĕێ����܂��B
        // �������A�e�I�u�W�F�N�g�̃��b�V����GetComponentsInChildren�Ɋ܂܂��̂ŏ��O���܂��B
        MeshFilter[] meshFilters = fieldParent.GetComponentsInChildren<MeshFilter>();
        List<MeshFilter> meshFilterList = new List<MeshFilter>();
        for (int i = 1; i < meshFilters.Length; i++)
        {
            meshFilterList.Add(meshFilters[i]);
        }

        // �������郁�b�V���̔z����쐬���܂��B
        CombineInstance[] combine = new CombineInstance[meshFilterList.Count];

        // �������郁�b�V���̏���CombineInstance�ɒǉ����Ă����܂��B
        for (int i = 0; i < meshFilterList.Count; i++)
        {
            combine[i].mesh = meshFilterList[i].sharedMesh;
            combine[i].transform = meshFilterList[i].transform.localToWorldMatrix;
            meshFilterList[i].gameObject.SetActive(false);
        }

        // �����������b�V�����Z�b�g���܂��B
        parentMeshFilter.mesh = new Mesh();
        parentMeshFilter.mesh.CombineMeshes(combine);

        // �����������b�V���Ƀ}�e���A�����Z�b�g���܂��B
        parentMeshRenderer.material = combinedMat;

        // �e�I�u�W�F�N�g��\�����܂��B
        fieldParent.gameObject.SetActive(true);
    }

    /// <Summary>
    /// �w�肳�ꂽ�R���|�[�l���g�ւ̎Q�Ƃ��擾���܂��B
    /// �R���|�[�l���g���Ȃ��ꍇ�̓A�^�b�`���܂��B
    /// </Summary>
    T CheckParentComponent<T>(GameObject obj) where T : Component
    {
        // �^�p�����[�^�Ŏw�肵���R���|�[�l���g�ւ̎Q�Ƃ��擾���܂��B
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
        // �e�I�u�W�F�N�g�̃R���|�[�l���g���擾���ATransform�ȊO�̃R���|�[�l���g���f�^�b�`���܂��B
        foreach (Component comp in obj.GetComponents<Component>())
        {
            if (comp.GetType() != typeof(Transform))
            {
                Destroy(comp);
            }
        }
    }
}
