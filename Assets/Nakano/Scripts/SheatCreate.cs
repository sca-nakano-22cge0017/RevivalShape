using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SheatCreate : MonoBehaviour
{
    [SerializeField] GameObject sheatPrefab;
    [SerializeField] Transform sheatParent;
    [SerializeField] Transform marks;

    [SerializeField] GameObject frontMark;
    [SerializeField] GameObject backMark;

    [SerializeField] StageController stageController;

    Vector3 mapSize;

    bool isCreated = false;

    public void Sheat()
    {
        if(isCreated) return;

        mapSize = stageController.MapSize; // �T�C�Y���

        // �}�b�v�̍L�����V�[�g���𐶐�
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                Instantiate(sheatPrefab, new Vector3(-x, -0.5f, z), Quaternion.identity, sheatParent);
            }
        }

        frontMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, mapSize.z + 0.5f);
        backMark.transform.position = new Vector3(-mapSize.x / 2 + 0.5f, -0.5f, -1.5f);

        isCreated = true;
    }

    /// <summary>
    /// �V�[�g�A�}�[�N�̕\���E��\��
    /// </summary>
    /// <param name="isDisp">true�̂Ƃ��V�[�g�\��</param>
    /// <param name="isMarkDisp">true�̂Ƃ��}�[�N�\��</param>
    public void SheatDisp(bool isDisp, bool isMarkDisp)
    {
        sheatParent.gameObject.SetActive(isDisp);
        marks.gameObject.SetActive(isMarkDisp);
    }
}
