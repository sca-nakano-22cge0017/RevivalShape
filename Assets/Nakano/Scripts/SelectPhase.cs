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

    SelectPhaseButton[,] selectButtons; // �e�{�^���̃f�[�^

    int[,,] _playerInputData; // ���̓f�[�^
    int selectShapeNum = 1; // �I�𒆂̐}�`

    ShapeData.Shape[] shapeType; // �g�p�}�`�̎��
    private int shapeTypeAmount = 0; // �g�p�}�`�̎�ސ�

    bool firstInput = true; // ��ԍŏ��̓��͂��ǂ���

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer;

    // �������[�h
    [SerializeField] GameObject eraserModeWindow;
    bool isEraser = false;
    public bool IsEraser { get { return isEraser; } }

    // �}�`�ύX
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // �E�B���h�E��\��
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);

        // �}�`�ύX�{�^���̔�\��
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
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

        // �}�`�ύX�{�^���@�g���}�`�̃{�^���̂ݕ\������
        ShapeChangeButtonsSet();

        selectShapeNum = 1;

        if (!firstInput)
        {
            // �𓚏�����
            ShapeArrayInitialize();
            stageController.PlayerAnswer = playerAnswer;

            InputDataToButton();
            return;
        }

        ArraysCreate();
        ButtonsCreate();

        firstInput = false;
    }

    /// <summary>
    /// �I���t�F�[�Y�I��
    /// </summary>
    public void SelectPhaseEnd()
    {
        InputNumSave();
        IntArrayToShapeArray();

        // �E�B���h�E�����
        selectPhaseUI.SetActive(false);

        // �v���C���[�̓�����ۑ�
        stageController.PlayerAnswer = playerAnswer;
    }

    /// <summary>
    /// �z��v�f���w��
    /// </summary>
    void ArraysCreate()
    {
        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

        // �z��@�v�f���w��
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        _playerInputData = new int[shapeTypeAmount, (int)mapSize.x, (int)mapSize.z];
        InputNumDataInitialize();

        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(mapSize.x * 200, mapSize.z * 200);

        ShapeArrayInitialize();
    }

    /// <summary>
    /// �{�^���̐���
    /// </summary>
    void ButtonsCreate()
    {
        // �}�b�v�̍L�����{�^���𐶐�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
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

    /// <summary>
    /// �I�𒆂̐}�`��ύX �{�^���ŌĂяo��
    /// </summary>
    /// <param name="shape">�I������}�`�̖��O</param>
    public void ShapeChange(string shapeName)
    {
        int n = selectShapeNum;
        ShapeData.Shape shape = (ShapeData.Shape)n;

        InputNumSave(); // ���̓f�[�^�ۑ�

        // �񋓌^��S����
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // �ł����܂ꂽstring�Ɠ����Ȃ�ύX����
            if(name.ToLower() == shapeName.ToLower())
            {
                selectShapeNum = (int)value;
                InputDataToButton();
            }
        }
    }

    /// <summary>
    /// �}�`�ύX�{�^���̕\���E��\��
    /// �g��Ȃ��}�`�͔�\���ɂ���
    /// </summary>
    void ShapeChangeButtonsSet()
    {
        shapeType = stageController.ShapeType; // �g�p�}�`���擾

        for(int b = 0; b < shapeChangeButtons.Length; b++)
        {
            string buttonsName = shapeChangeButtons[b].name;
            buttonsName.ToLower();

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);
                needShapeName.ToLower();
                
                if (buttonsName == needShapeName)
                {
                    shapeChangeButtons[b].SetActive(true);
                    break;
                }
            }
        }
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
                _playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// ���̓f�[�^��������
    /// </summary>
    /// <param name="num">����������z��</param>
    void InputNumDataInitialize()
    {
        for(int n = 0; n < shapeTypeAmount; n++)
        {
            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    _playerInputData[n, x, z] = 0;
                }
            }
        }
    }

    /// <summary>
    /// �{�^���̕\����ύX
    /// </summary>
    /// <param name="isReset">�l��0�ɂ��邩</param>
    void InputDataToButton()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z].InputNum = _playerInputData[selectShapeNum, z, x];
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
        ShapeData.Shape shape;

        for (int n = 0; n < shapeTypeAmount; n++)
        {
            shape = (ShapeData.Shape) n;

            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    int nextYPos = 0;

                    // �󔒃}�X���������� 1�i�ڂ��猟�����Ă����A�󔒃}�X���������玟�̐}�`�����������֔z�u���Ă���
                    for (int y = 0; y < (int)mapSize.y; y++)
                    {
                        if (playerAnswer[x, y, z] != ShapeData.Shape.Empty) continue;
                        else
                        {
                            nextYPos = y;
                            break;
                        }
                    }

                    // playerInputData�ɂ�Y�������ɐςސ��������Ă���
                    for (int y = 0; y < _playerInputData[n, z, x]; y++)
                    {
                        playerAnswer[x, y + nextYPos, z] = shape;
                    }
                }
            }
        }
    }
}
