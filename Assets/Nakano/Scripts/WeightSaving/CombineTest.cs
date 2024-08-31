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

        // ���b�V������������Q�[���I�u�W�F�N�g���擾
        int length = mCombineObj.transform.childCount;
        mObjArray = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            mObjArray[i] = mCombineObj.transform.GetChild(i).gameObject;
        }

        // MeshBaker�ɓo�^
        meshbaker.AddDeleteGameObjects(mObjArray, null, false);

        // �o�^����Ă���Q�[���I�u�W�F�N�g�̃��b�V�����������A�V�[�����ɏo��
        meshbaker.Apply();

        // �������ƂȂ�Q�[���I�u�W�F�N�g���폜����
        Destroy(mCombineObj);
    }
}
