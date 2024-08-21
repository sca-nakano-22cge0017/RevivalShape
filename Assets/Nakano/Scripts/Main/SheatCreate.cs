using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �V�[�g�Ǘ�
/// </summary>
public class SheatCreate : MonoBehaviour
{
    [SerializeField] private StageController stageController;

    [SerializeField] private GameObject sheatPrefab;
    [SerializeField] private Transform sheatParent;
    [SerializeField] private GameObject marks;

    [SerializeField, Header("�w�O�x�̃}�[�N")] private GameObject frontMark;
    [SerializeField, Header("�w��x�̃}�[�N")] private GameObject backMark;

    private Vector3 mapSize;

    private bool isCreated = false;
    private const float markPosY = -0.5f;

    /// <summary>
    /// �V�[�g�쐬
    /// </summary>
    public void Create()
    {
        // �ēx�������Ȃ��悤�ɂ���
        if(isCreated) return;

        // �T�C�Y�擾
        if (stageController)
            mapSize = stageController.MapSize;

        // �}�b�v�̍L�����V�[�g���𐶐�
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                Instantiate(sheatPrefab, new Vector3(-x, -0.5f, z), Quaternion.identity, sheatParent);
            }
        }

        // �O��������}�[�N�̈ʒu��ݒ�
        var markPosX = -mapSize.x / 2 + 0.5f;
        frontMark.transform.position = new Vector3(markPosX, markPosY, mapSize.z + 1.5f);
        backMark.transform.position  = new Vector3(markPosX, markPosY, -2.5f);

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
        marks.SetActive(isMarkDisp);
    }
}
