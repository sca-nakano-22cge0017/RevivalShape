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
        // �{�^���̃E�B���h�E�͕��Ă���
        buttonParent.SetActive(false);

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z��@�v�f���w��
        //! �ő�l�͑S�X�e�[�W���ꂵ���肷�邩���H
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        ShapeArrayInitialize();

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

    }

    /// <summary>
    /// �{�^���ɓ��͂������l��z��ɕۑ�����
    /// </summary>
    void InputNumSave()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // ���͂�������int�^�̔z��ɑ��
                playerInputData[x, z] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// �{�^���ɓ��͂������l��S��0�ɖ߂�
    /// </summary>
    void InputNumReset()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z].InputNum = 0;
            }
        }
    }

    /// <summary>
    /// �z�u�f�[�^�̏�����
    /// </summary>
    void ShapeArrayInitialize()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                for (int y = 0; y < (int)mapSize.y; y++)
                {
                    // �Ƃ肠�����󔒃}�X�Ŗ��߂�
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
    }

    /// <summary>
    /// int�^�̔z�񂩂�Shape�^�̎O�����z��ɕϊ�����
    /// </summary>
    void IntArrayToShapeArray()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // playerInputData�ɂ�Y�������ɐςސ��������Ă���
                for (int y = 0; y < playerInputData[x, z]; y++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Cube;
                }
            }
        }
    }

    public void SelectPhaseStart()
    {
        // �E�B���h�E���J��
        buttonParent.SetActive(true);
    }

    public void SelectPhaseEnd()
    {
        InputNumSave();

        InputNumReset();

        IntArrayToShapeArray();

        // �E�B���h�E�����
        buttonParent.SetActive(false);

        // �v���C���[�̓�����ۑ�
        stageController.PlayerAnswer = playerAnswer;
    }
}
