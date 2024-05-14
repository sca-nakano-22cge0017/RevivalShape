using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField] GameObject selectPhaseUI;

    bool buttonsCreated = false; // �{�^���쐬�ς݂��ǂ���
    SelectPhaseButton[,] selectButtons; // �e�{�^���̃f�[�^
    int[,] playerInputData; // ���̓f�[�^

    //! ���쒆
    List<SelectPhaseButton[,]> _selectButtons; // �e�{�^���̃f�[�^
    ShapeData.Shape selectShape; // �I�𒆂̐}�`

    bool arraysCreated = false; // �z��̗v�f�����w��ς݂��ǂ���
    bool firstInput = true; // ��ԍŏ��̓��͂��ǂ���

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    // �������[�h
    [SerializeField] GameObject eraserModeWindow;
    bool isEraser = false;
    public bool IsEraser { get { return isEraser; } }

    private void Awake()
    {
        // �E�B���h�E��\��
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);
    }

    private void Update()
    {
        if (isEraser) eraserModeWindow.SetActive(true);
        else eraserModeWindow.SetActive(false);
    }

    /// <summary>
    /// �I���t�F�[�Y�ڍs���̏���
    /// </summary>
    public void SelectPhaseStart()
    {
        // UI�\��
        selectPhaseUI.SetActive(true);

        ArraysCreate();
        
        ButtonsCreate();
    }

    /// <summary>
    /// �I���t�F�[�Y�I��
    /// </summary>
    public void SelectPhaseEnd()
    {
        InputNumSave();

        InputNumReset();

        IntArrayToShapeArray();

        // �E�B���h�E�����
        //buttonParent.SetActive(false);
        selectPhaseUI.SetActive(false);

        // �v���C���[�̓�����ۑ�
        stageController.PlayerAnswer = playerAnswer;
        firstInput = false;
    }

    /// <summary>
    /// �z��v�f���w��
    /// </summary>
    void ArraysCreate()
    {
        if(arraysCreated) return;

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z��@�v�f���w��
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize.x * 200, mapSize.z * 200);

        ShapeArrayInitialize();

        arraysCreated = true;
    }

    /// <summary>
    /// �{�^���̐���
    /// </summary>
    void ButtonsCreate()
    {
        if(buttonsCreated) return;

        // �}�b�v�̍L�����{�^���𐶐�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
            }
        }

        buttonsCreated = true;
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
                playerInputData[z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// �{�^���ɓ��͂������l��S��0�ɖ߂�
    /// </summary>
    void InputNumReset()
    {
        if (!firstInput) return;

        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // ����Ȃ�0�����Ă���
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
                for (int y = 0; y < playerInputData[z, x]; y++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Cube;
                }
            }
        }
    }

    /// <summary>
    /// �������[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void EraserMode()
    {
        isEraser = !isEraser;
    }
}
