using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// ���b�V�������A�e�N�X�`����ݒ肷��
/// DrawCall�����炷���߂̋@�\ �}�`���ɃX�N���v�g��p�ӂ���K�v������
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
    /// ���b�V�������E�e�N�X�`���̒ǉ�
    /// </summary>
    /// <param name="_stageName">�X�e�[�W��</param>
    /// <param name="_shape">�}�`</param>
    /// <param name="_parent">��������u���b�N�̐e�I�u�W�F�N�g</param>
    public void Combine(string _stageName, ShapeData.Shape _shape, Transform _parent)
    {
        StartCoroutine(CombineCoroutine(_stageName, _shape, _parent));
    }

    /// <summary>
    /// �e�N�X�`���̎擾 Addressable���g�p
    /// </summary>
    /// <param name="_stageName"></param>
    /// <param name="_shape"></param>
    void GetTexture(string _stageName, ShapeData.Shape _shape)
    {
        string fileName = fileFormat1 + _stageName + "/" + _shape + fileFormat2;
        AsyncOperationHandle<MB2_TextureBakeResults> m_TextHandle;

        // �}�b�v�T�C�Y�̃f�[�^���擾
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

        // ���b�V������������Q�[���I�u�W�F�N�g���擾
        int length = _parent.transform.childCount;
        mObjArray = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            mObjArray[i] = _parent.transform.GetChild(i).gameObject;
        }

        // MeshBaker�ɓo�^
        meshbaker.AddDeleteGameObjects(mObjArray, null, false);

        // �o�^����Ă���Q�[���I�u�W�F�N�g�̃��b�V�����������A�V�[�����ɏo��
        meshbaker.Apply();

        // �������ƂȂ�Q�[���I�u�W�F�N�g���폜����
        _parent.gameObject.SetActive(false);
    }

    IEnumerator CombineCoroutine(string _stageName, ShapeData.Shape _shape, Transform _parent)
    {
        GetTexture(_stageName, _shape);

        yield return new WaitUntil(() => isTextureLoaded);

        CombineMesh(_parent);
    }
}
