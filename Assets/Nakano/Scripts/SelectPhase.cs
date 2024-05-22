using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �I���t�F�[�Y
/// </summary>
public class SelectPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] GameObject buttonParent;
    [SerializeField] GameObject buttonPrefab;

    [SerializeField, Header("�{�^���\���͈�")] Vector2 buttonRange;

    [SerializeField] GameObject selectPhaseUI;

    SelectPhaseButton[,] selectButtons; // �e�{�^���̃f�[�^

    int[,,] playerInputData; // ���̓f�[�^ [�}�`�̎��, X���W, Z���W]
    int selectShapeNum = 1;  // �I�𒆂̐}�`

    ShapeData.Shape[] shapeType;     // �g�p�}�`�̎��
    private int shapeTypeAmount = 0; // �g�p�}�`�̎�ސ�

    bool firstInput = true; // ��ԍŏ��̓��͂��ǂ���

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer; // �v���C���[�̉�

    // �������[�h
    [SerializeField] GameObject eraserModeWindow;
    public bool IsEraser { get; private set; } = false;

    // �m�F���[�h
    [SerializeField] GameObject checkModeWindow;
    [SerializeField] GameObject stepsParent;
    [SerializeField] GameObject stepsPrefab;
    Image[] steps;
    [SerializeField] Sprite[] shapeIcon;
    public bool IsCheck { get; set; } = false;
    Vector2 checkPos = new Vector2(0, 0); // �m�F����}�X
    int stepsAmount = 5;

    // �m�F�J�������[�h�̃E�B���h�E�������ł��邩
    bool canCheckWindowUnDisp = false;

    // �e���[�h���̍��w�i
    [SerializeField] GameObject modeBG;

    // �}�`�ύX
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // �E�B���h�E�AUI��\��
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);

        // �}�`�ύX�{�^���̔�\��
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
    }

    private void Update()
    {
        // ���[�h���͔w�i�Â�
        if(IsEraser || IsCheck) modeBG.SetActive(true);
        else modeBG.SetActive(false);

        // ��ʃ^�b�v�Ŋm�F�J�������[�h�̃E�B���h�E�����
        if(canCheckWindowUnDisp && Input.GetMouseButton(0))
        {
            checkModeWindow.SetActive(false);
            canCheckWindowUnDisp = false;

            selectButtons[(int)checkPos.x, (int)checkPos.y].IsCheck = false;
        }
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

        // ����łȂ��Ƃ�
        if (!firstInput)
        {
            // �𓚏�����
            ShapeArrayInitialize();
            stageController.PlayerAnswer = playerAnswer;

            InputDataToButton();
            return;
        }

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        ArraysCreate();
        UISetting();

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
        // �z��@�v�f���w��
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[shapeTypeAmount, (int)mapSize.x, (int)mapSize.z];
        //steps = new Image[(int)mapSize.y];
        steps = new Image[stepsAmount];

        InputNumDataInitialize();
        ShapeArrayInitialize();
    }

    /// <summary>
    /// UI�̃T�C�Y�Ȃǂ�ݒ�
    /// </summary>
    void UISetting()
    {
        // �{�^���̐e�I�u�W�F�N�g�̃T�C�Y����
        buttonParent.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonRange.x, buttonRange.y);
        buttonParent.GetComponent<GridLayoutGroup>().cellSize = new Vector2(buttonRange.x / mapSize.x, buttonRange.y / mapSize.z);

        // �}�b�v�̍L�����{�^���𐶐�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                selectButtons[x, z] = Instantiate(buttonPrefab, buttonParent.transform).GetComponent<SelectPhaseButton>();
            }
        }

        // �m�F�J�������[�h�̃E�B���h�E����
        for (int y = 0; y < /*(int)mapSize.y*/ stepsAmount; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / /*mapSize.y*/ stepsAmount);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // �������Hierarchy�̈�ԏ�ɐݒ肷�� �� �����珇�ɐ�������
            steps[y].transform.SetAsFirstSibling();
        }
    }

    /// <summary>
    /// �������[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void EraserModeChange()
    {
        // �m�F���[�h�Ȃ疳��
        if (IsCheck) return;

        IsEraser = !IsEraser;

        if (IsEraser) eraserModeWindow.SetActive(true);
        else eraserModeWindow.SetActive(false);
    }

    /// <summary>
    /// �m�F���[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void CheckModeChange()
    {
        // �������[�h�A�E�B���h�E�\�����Ȃ疳��
        if (IsEraser || checkModeWindow.activeSelf) return;

        IsCheck = !IsCheck;
    }

    /// <summary>
    /// �m�F�J�������[�h �w�肵���}�X�̐}�`��\������
    /// </summary>
    public void CheckWindowDisp()
    {
        // �E�B���h�E�\��
        checkModeWindow.SetActive(true);

        // �摜������
        for (int i = 0; i < steps.Length; i++)
        {
            Sprite s = shapeIcon[0]; // �摜�擾
            steps[i].sprite = s; // �摜�ύX
        }

        InputNumSave();
        IntArrayToShapeArray();

        checkPos = new Vector2(0, 0); // �m�F����}�X
        bool isSearchEnd = false;

        // �m�F����}�X���ǂꂩ�𒲂ׂ�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // �m�F����}�X������������for���𔲂���
                if(selectButtons[x, z].IsCheck)
                {
                    checkPos = new Vector2(x, z);
                    isSearchEnd = true;
                    break;
                }
            }

            if(isSearchEnd) break;
        }

        for(int i = 0; i < steps.Length; i++)
        {
            // �z�u����Ă���}�`���擾
            var shape = (int)playerAnswer[(int)checkPos.x, i, (int)checkPos.y];

            Sprite s = shapeIcon[shape];  // �摜�擾
            steps[i].sprite = s;          // �摜�ύX
        }

        StartCoroutine(CheckWindowUnDisp());
    }

    /// <summary>
    /// �E�B���h�E�̕\���Ɣ�\������u�ōs����̂ŕ\�����莞�ԑ҂��Ă���A��\���ɂł���悤�ɂ���
    /// </summary>
    IEnumerator CheckWindowUnDisp()
    {
        yield return new WaitForSeconds(0.1f);
        canCheckWindowUnDisp = true;
    }

    /// <summary>
    /// �I�𒆂̐}�`��ύX �{�^���ŌĂяo��
    /// </summary>
    /// <param name="shape">�I������}�`�̖��O</param>
    public void ShapeChange(string shapeName)
    {
        InputNumSave(); // ���̓f�[�^�ۑ�

        // �񋓌^��S����
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // ���O���ł����܂ꂽstring�Ɠ����Ȃ�A�I�����Ă���}�`��ύX����
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
        // �g�p���Ă���}�`�̎�ސ����擾
        shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

        // �g�p�}�`���擾
        shapeType = stageController.ShapeType;

        bool firstShape = true; // ��ԍŏ��ɔz�u����}�`

        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            // �{�^���̖��O���擾
            string buttonsName = shapeChangeButtons[b].name;
            buttonsName.ToLower();

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);
                needShapeName.ToLower();
                
                // �{�^���̖��O�Ǝg�p���Ă���}�`�̎�ނ�����Ă�����̂������
                if (buttonsName == needShapeName)
                {
                    // �{�^����\��
                    shapeChangeButtons[b].SetActive(true);

                    if(firstShape)
                    {
                        // ������ԂőI�����Ă���}�`��ݒ�
                        selectShapeNum = (int)shapeType[st];
                        firstShape = false;
                    }

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
                playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
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
                    playerInputData[n, x, z] = 0;
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
                // ���͂���Ă����l���{�^���ɔ��f����
                selectButtons[x, z].InputNum = playerInputData[selectShapeNum, z, x];
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
                    // ������Ă���
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
        // ��x����������
        ShapeArrayInitialize();

        ShapeData.Shape shape;

        for (int n = 0; n < shapeTypeAmount; n++)
        {
            shape = (ShapeData.Shape) n;

            for (int z = 0; z < (int)mapSize.z; z++)
            {
                for (int x = 0; x < (int)mapSize.x; x++)
                {
                    // ���ɃI�u�W�F�N�g��z�u����ʒu
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
                    for (int y = 0; y < playerInputData[n, z, x]; y++)
                    {
                        playerAnswer[x, y + nextYPos, z] = shape;
                    }
                }
            }
        }
    }
}
