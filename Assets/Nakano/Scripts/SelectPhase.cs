using System.Collections;
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

    int[,] playerInputData;
    int selectShapeNum = 1;  // �I�𒆂̐}�`

    ShapeData.Shape[] shapeType;     // �g�p�}�`�̎��

    bool firstInput = true; // ��ԍŏ��̓��͂��ǂ���

    Vector3 mapSize;

    ShapeData.Shape[,,] playerAnswer; // �v���C���[�̉�

    // �폜���[�h
    [SerializeField] GameObject eraserModeButton;
    public bool IsEraser { get; set; } = false;

    // �m�F���[�h
    [SerializeField] GameObject checkModeButton;
    [SerializeField] GameObject checkModeWindow;
    [SerializeField] GameObject stepsParent;
    [SerializeField] GameObject stepsPrefab;
    Image[] steps;
    [SerializeField] Sprite[] shapeIcon;
    public bool IsCheck { get; set; } = false;
    Vector2 checkPos = new Vector2(0, 0); // �m�F����}�X

    // �m�F�J�������[�h�̃E�B���h�E�������ł��邩
    bool canCheckWindowUnDisp = false;

    // �X���C�v���Đ������㏸�������邩
    public bool CanSwipInput { get; set; } = false;

    // �e���[�h���̍��w�i
    [SerializeField] GameObject modeBG;

    [SerializeField] GameObject clearBG;

    // �}�`�ύX
    [SerializeField] GameObject[] shapeChangeButtons;

    private void Awake()
    {
        // �E�B���h�E�AUI��\��
        selectPhaseUI.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);
        clearBG.SetActive(false);

        // �}�`�ύX�{�^���̔�\��
        for (int b = 0; b < shapeChangeButtons.Length; b++)
        {
            shapeChangeButtons[b].SetActive(false);
        }
    }

    private void Update()
    {
        UIControl();
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
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        steps = new Image[(int)mapSize.y];

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
                var button = Instantiate(buttonPrefab, buttonParent.transform);

                selectButtons[x, z] = button.GetComponent<SelectPhaseButton>();
                selectButtons[x, z].Position = new Vector2(x, z);
                selectButtons[x, z].Input_max = (int)mapSize.y;
            }
        }

        // �m�F�J�������[�h�̃E�B���h�E����
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // �������Hierarchy�̈�ԏ�ɐݒ肷�� �� �����珇�ɐ�������
            steps[y].transform.SetAsFirstSibling();
        }
    }

    /// <summary>
    /// UI����
    /// </summary>
    void UIControl()
    {
        if (IsEraser || IsCheck)
        {
            // ���[�h���͔w�i�Â�
            modeBG.SetActive(true);
            clearBG.SetActive(true);

            // �{�^���̊O���������烂�[�h�I��
            if (Input.GetMouseButtonDown(0))
            {
                Vector2 mPos = Input.mousePosition;
                float minX = Screen.width / 2 - buttonRange.x / 2;
                float maxX = Screen.width - minX;
                float minY = Screen.height / 2 - buttonRange.y / 2;
                float maxY = Screen.height - minY;

                if (mPos.x < minX || mPos.x > maxX || mPos.y < minY || mPos.y > maxY)
                {
                    if (IsEraser) IsEraser = false;
                    if (IsCheck) IsCheck = false;
                }
            }
        }
        else
        {
            modeBG.SetActive(false);
            clearBG.SetActive(false);
        }

        // �{�^���̃��C���[�ʒu����
        if (IsEraser)
        {
            checkModeButton.transform.SetAsFirstSibling();
            eraserModeButton.transform.SetAsLastSibling();
        }
        if (IsCheck)
        {
            eraserModeButton.transform.SetAsFirstSibling();
            checkModeButton.transform.SetAsLastSibling();
        }

        // ��ʃ^�b�v�Ŋm�F�J�������[�h�̃E�B���h�E�����
        if (canCheckWindowUnDisp && Input.GetMouseButtonDown(0))
        {
            checkModeWindow.SetActive(false);
            canCheckWindowUnDisp = false;

            selectButtons[(int)checkPos.x, (int)checkPos.y].IsCheck = false;
        }
    }

    /// <summary>
    /// �������[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void EraserModeChange()
    {
        IsEraser = !IsEraser;
    }

    /// <summary>
    /// �m�F���[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void CheckModeChange()
    {
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

        checkPos = new Vector2(0, 0); // �m�F����}�X
        bool isSearchEnd = false;

        // �m�F����}�X���ǂꂩ�𒲂ׂ�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // �m�F����}�X������������for���𔲂���
                if (selectButtons[x, z].IsCheck)
                {
                    checkPos = new Vector2(x, z);
                    isSearchEnd = true;
                    break;
                }
            }

            if (isSearchEnd) break;
        }

        for (int i = 0; i < steps.Length; i++)
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
        // �񋓌^��S����
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // ���O���ł����܂ꂽstring�Ɠ����Ȃ�A�I�����Ă���}�`��ύX����
            if (name.ToLower() == shapeName.ToLower())
            {
                selectShapeNum = (int)value;
            }
        }
    }

    /// <summary>
    /// �}�`�ύX�{�^���̕\���E��\��
    /// �g��Ȃ��}�`�͔�\���ɂ���
    /// </summary>
    void ShapeChangeButtonsSet()
    {
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

                    if (firstShape)
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
                //playerInputData[selectShapeNum, z, x] = selectButtons[x, z].InputNum;
                playerInputData[z, x] = selectButtons[x, z].InputNum;
            }
        }
    }

    /// <summary>
    /// ���̓f�[�^��������
    /// </summary>
    /// <param name="num">����������z��</param>
    void InputNumDataInitialize()
    {
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                playerInputData[x, z] = 0;
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
                selectButtons[x, z].InputNum = playerInputData[z, x];
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
    /// �}�`����
    /// </summary>
    /// <param name="buttonPos">���͂���}�X</param>
    public void ShapeInput(Vector2 buttonPos)
    {
        // ���ɃI�u�W�F�N�g��z�u����ʒu
        int nextYPos = 0;

        // �󔒃}�X���������� 1�i�ڂ��猟�����Ă����A�󔒃}�X���������玟�̐}�`�����������֔z�u���Ă���
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            if (playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] != ShapeData.Shape.Empty) continue;
            else
            {
                nextYPos = y;
                break;
            }
        }

        playerAnswer[(int)buttonPos.x, nextYPos, (int)buttonPos.y] = (ShapeData.Shape)selectShapeNum;
    }

    /// <summary>
    /// �폜���[�h�@�}�`�폜
    /// </summary>
    /// <param name="buttonPos">�폜����}�X</param>
    public void ShapeDelete(Vector2 buttonPos)
    {
        // �ォ�珇�ɍ폜
        for (int y = (int)mapSize.y - 1; y >= 0; y--)
        {
            // �󔒃}�X�͔�΂�
            if (playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] == ShapeData.Shape.Empty) continue;
            else
            {
                playerAnswer[(int)buttonPos.x, y, (int)buttonPos.y] = ShapeData.Shape.Empty;
                break;
            }
        }
    }
}
