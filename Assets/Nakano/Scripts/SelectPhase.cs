using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    SelectPhaseButton[,] selectButtons; // �e�{�^���̃f�[�^
    int[,] playerInputData; // ���̓f�[�^

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    void Start()
    {
        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z��@�v�f���w��
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        // �}�b�v�̍L�����{�^���𐶐�
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
            }
        }
    }

    void Update()
    {
        // debug
        if(Input.GetKeyDown(KeyCode.Return))
        {
            InputNumSave();
        }
    }

    void InputNumSave()
    {
        for (int x = 0; x < (int)mapSize.x; x++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                // ���͂�������int�^�̔z��ɑ��
                playerInputData[x, z] = selectButtons[x, z].InputNum;
            }
        }
    }
}
