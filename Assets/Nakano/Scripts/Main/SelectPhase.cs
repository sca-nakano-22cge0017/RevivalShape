using UnityEngine;
using UnityEngine.UI;
using static Extensions;

namespace select
{
    /// <summary>
    /// �e�}�`����2D�摜�A�}�`�ύX���̃{�^��
    /// </summary>
    [System.Serializable]
    public class ShapesUI
    {
        public ShapeData.Shape shape; // �}�`
        public Sprite sprite;         // �A�C�R���Ɏg�p����摜
        public GameObject button;     // �}�`�ύX�{�^��
    }
}

/// <summary>
/// �I���t�F�[�Y
/// </summary>
public class SelectPhase : MonoBehaviour, IPhase
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;

    // ���̓{�^��
    [SerializeField] private GameObject buttonParent;
    [SerializeField] private RectTransform buttonParent_rc;
    [SerializeField] private GridLayoutGroup buttonParent_glg;
    [SerializeField] private GameObject buttonPrefab;
    [SerializeField, Header("�{�^���\���͈�")] private Vector2 buttonRange;

    [SerializeField] private GameObject selectPhaseUI;

    private SelectPhaseButton[,] selectButtons; // �e�{�^���ɓ��͂��ꂽ�f�[�^
    private int[,] playerInputData;             // �v���C���[�����͂����f�[�^

    private ShapeData.Shape selectingShape;     // �I�𒆂̐}�`
    [SerializeField, Header("�I�𒆂̐}�`")] Image selectingShapeImage;
    private ShapeData.Shape[] shapeType;        // �g�p�}�`�̎��

    private Vector3 mapSize;

    private ShapeData.Shape[,,] playerAnswer;   // �v���C���[�̉�

    // �e�}�`����UI
    [SerializeField] private select.ShapesUI[] shapesUI;

    // �폜���[�h
    [SerializeField, Header("�폜���[�h�̃{�^��")] private GameObject eraserModeButton;
    [SerializeField] private Image eraserModeButton_img;
    [SerializeField, Header("�폜���[�h�̃E�B���h�E")] private GameObject eraserModeWindow;
    /// <summary>
    /// �폜���[�h��
    /// </summary>
    public bool IsEraser { get; set; } = false;
    private bool canEraserWindowUnDisp = false;

    // �m�F���[�h
    [SerializeField, Header("�m�F���[�h�̃{�^��")] private GameObject checkModeButton;
    [SerializeField] private Image checkModeButton_img;
    [SerializeField, Header("�m�F���[�h�̃E�B���h�E")] private GameObject checkModeWindow;
    [SerializeField] private GameObject stepsParent;       // �摜��z�u����ꏊ
    [SerializeField] private RectTransform stepsParent_rc; // stepsParent��RectTransform
    [SerializeField] private GameObject stepsPrefab;       // ��������摜
    private Image[] steps;                                 // �\������摜�@Sprite��ύX����
    /// <summary>
    /// �m�F���[�h��
    /// </summary>
    public bool IsCheck { get; private set; } = false;
    private Vector2 checkPos = Vector2.zero;    // �m�F����}�X�̍��W

    // �m�F�J�������[�h�̃E�B���h�E�������ł��邩
    private bool canCheckWindowUnDisp = false;

    private bool canSwipInput = false;
    /// <summary>
    /// �X���C�v���Đ������㏸�������邩
    /// </summary>
    public bool CanSwipInput
    {
        get
        {
            return canSwipInput;
        }
        set
        {
            canSwipInput = value;
        }
    }

    private bool isLongTap = false;
    /// <summary>
    /// �{�^���𒷉������Ă��邩
    /// </summary>
    public bool IsLongTap { get { return isLongTap; } set { isLongTap = value; } }

    // �S����
    [SerializeField, Header("�S�����m�F�E�B���h�E")] private GameObject confirmWindow;
    [SerializeField, Header("�S�������@�\�����邩")] private bool canReset = true;

    [SerializeField, Header("�e���[�h���̍��w�i")] private GameObject modeBG;

    public void Initialize()
    {
        // �E�B���h�E�AUI��\��
        selectPhaseUI.SetActive(false);
        eraserModeWindow.SetActive(false);
        checkModeWindow.SetActive(false);
        confirmWindow.SetActive(false);
        modeBG.SetActive(false);

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �g�p�}�`���擾
        shapeType = stageController.ShapeType;

        // �{�^���̐e�I�u�W�F�N�g�̃T�C�Y����
        buttonParent_rc.sizeDelta = new Vector2(buttonRange.x, buttonRange.y);
        buttonParent_glg.cellSize = new Vector2(buttonRange.x / mapSize.x, buttonRange.y / mapSize.z);

        // �z��̗v�f���ݒ�
        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        // �z��̏�����
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerInputData[x, z] = 0;
                }
            }
        }

        // �}�b�v�̍L�����{�^���𐶐�
        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                var button = Instantiate(buttonPrefab, buttonParent.transform);

                selectButtons[x, z] = button.GetComponent<SelectPhaseButton>();
                selectButtons[x, z].Position = new Vector2(x, z);
                selectButtons[x, z].InputMax = (int)mapSize.y;
                selectButtons[x, z].selectPhase = this;
            }
        }

        // �m�F�J�������[�h�̃E�B���h�E�ݒ�
        steps = new Image[(int)mapSize.y];
        stepsParent_rc.transform.localPosition = new Vector2(0, 1105 + (mapSize.y - 4) * 110);
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // �������Hierarchy�̈�ԏ�ɐݒ肷�� �� �����珇�ɐ�������
            steps[y].transform.SetAsFirstSibling();
        }

        // �}�`�ύX�{�^���ݒ�
        bool firstShape = true;
        foreach (var ui in shapesUI)
        {
            if (ui.shape == ShapeData.Shape.Empty) continue;

            string buttonsName = ui.button.name;

            for (int st = 0; st < shapeType.Length; st++)
            {
                string needShapeName = ShapeData.Shape.GetName(typeof(ShapeData.Shape), shapeType[st]);

                // �{�^���̖��O�Ǝg�p���Ă���}�`�̎�ނ�����Ă�����̂������
                if (buttonsName.ToLower() == needShapeName.ToLower())
                {
                    // �{�^����\��
                    ui.button.SetActive(true);

                    // ��Ԗڂ̐}�`�Ȃ�
                    if (firstShape)
                    {
                        // ������ԂőI�����Ă���}�`��ݒ�
                        selectingShape = shapeType[st];
                        firstShape = false;
                    }

                    break;
                }
                else ui.button.SetActive(false);
            }
        }

        ShapeChangeButtonDispSet();
    }

    /// <summary>
    /// �I���t�F�[�Y�ڍs���̏���
    /// </summary>
    public void PhaseStart()
    {
        // UI�\��
        selectPhaseUI.SetActive(true);

        for (int z = 0; z < (int)mapSize.z; z++)
        {
            for (int x = 0; x < (int)mapSize.x; x++)
            {
                // ���͂���Ă����l���{�^���ɔ��f����
                selectButtons[x, z].InputNum = playerInputData[z, x];
            }
        }
    }

    public void PhaseUpdate()
    {
        // ��ʃ^�b�v�Ŋm�F���[�h�̃E�B���h�E�����
        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            if (canCheckWindowUnDisp)
            {
                checkModeWindow.SetActive(false);
                canCheckWindowUnDisp = false;

                selectButtons[(int)checkPos.x, (int)checkPos.y].ShapeCheckEnd();
            }

            if (canEraserWindowUnDisp)
            {
                eraserModeWindow.SetActive(false);
                canEraserWindowUnDisp = false;
            }
        }
    }

    /// <summary>
    /// �I���t�F�[�Y�I��
    /// </summary>
    public void PhaseEnd()
    {
        InputNumSave();

        // �E�B���h�E�����
        selectPhaseUI.SetActive(false);

        // �v���C���[�̓�����ۑ�
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    stageController.PlayerAnswer[x, y, z] = playerAnswer[x, y, z];
                }
            }
        }
    }

    Color clear = new Color(1, 1, 1, 0);
    Color unClear = new Color(1, 1, 1, 1);

    /// <summary>
    /// �e���[�h�̕\���ݒ�
    /// </summary>
    void ModeUIDispSet()
    {
        if (IsEraser || IsCheck)
        {
            // ���[�h���͔w�i�Â�
            modeBG.SetActive(true);

            Image disp = null;
            Image unDisp = null;

            if (IsCheck)
            {
                disp = checkModeButton_img;
                unDisp = eraserModeButton_img;
            }
            if (IsEraser)
            {
                unDisp = checkModeButton_img;
                disp = eraserModeButton_img;
            }

            // ���[�h���̓{�^���𔒂����点��
            disp.color = unClear;
            unDisp.color = clear;

            // �\�����ύX
            unDisp.transform.SetAsFirstSibling();
            disp.transform.SetAsLastSibling();
        }
        else
        {
            checkModeButton_img.color = clear;
            eraserModeButton_img.color = clear;

            modeBG.SetActive(false);
        }

        if (IsEraser && !eraserModeWindow.activeSelf && canReset)
            eraserModeWindow.SetActive(true);
        if (!IsEraser && eraserModeWindow.activeSelf && canReset)
            eraserModeWindow.SetActive(false);
    }

    /// <summary>
    /// �������[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void EraserModeChange()
    {
        IsEraser = !IsEraser;
        ModeUIDispSet();
    }

    /// <summary>
    /// �m�F���[�h�ƒʏ탂�[�h��؂�ւ�
    /// </summary>
    public void CheckModeChange()
    {
        IsCheck = !IsCheck;
        ModeUIDispSet();
    }

    public void ModeEnd()
    {
        if (IsEraser) IsEraser = false;
        if (IsCheck) IsCheck = false;

        ModeUIDispSet();
    }

    /// <summary>
    /// �m�F�J�������[�h �w�肵���}�X�̐}�`��\������
    /// </summary>
    public void CheckWindowDisp(Vector2 _checkPos)
    {
        checkPos = _checkPos;

        // �E�B���h�E�\��
        checkModeWindow.SetActive(true);

        // �摜������
        for (int i = 0; i < steps.Length; i++)
        {
            Sprite s = shapesUI[0].sprite;// �摜�擾
            steps[i].sprite = s; // �摜�ύX
        }

        InputNumSave();

        for (int i = 0; i < steps.Length; i++)
        {
            // �z�u����Ă���}�`���擾
            var shape = playerAnswer[(int)checkPos.x, i, (int)checkPos.y];

            // �摜�ύX
            for (int ui = 0; ui < shapesUI.Length; ui++)
            {
                if(shape == shapesUI[ui].shape)
                {
                    steps[i].sprite = shapesUI[ui].sprite;
                }
            }
        }

        // �E�B���h�E�̕\���Ɣ�\������u�ōs����̂ŕ\�����莞�ԑ҂��Ă���A��\���ɂł���悤�ɂ���
        StartCoroutine(DelayCoroutine(0.1f, () =>
        {
            canCheckWindowUnDisp = true;
        }));
    }

    /// <summary>
    /// �I�𒆂̐}�`��ύX �{�^���ŌĂяo��
    /// </summary>
    /// <param name="shape">�I������}�`�̖��O</param>
    public void ShapeChange(string shapeType)
    {
        // �񋓌^��S����
        foreach (ShapeData.Shape value in ShapeData.Shape.GetValues(typeof(ShapeData.Shape)))
        {
            string name = ShapeData.Shape.GetName(typeof(ShapeData.Shape), value);

            // ���O���ł����܂ꂽstring�Ɠ����Ȃ�A�I�����Ă���}�`��ύX����
            if (name.ToLower() == shapeType.ToLower())
            {
                selectingShape = value;
            }
        }

        ShapeChangeButtonDispSet();
    }

    /// <summary>
    /// �}�`�ύX�{�^���̕\���ύX
    /// </summary>
    void ShapeChangeButtonDispSet()
    {
        foreach (select.ShapesUI s in shapesUI)
        {
            if (s.shape == ShapeData.Shape.Empty) continue;

            // �I�𒆂̐}�`�͘g�\���{���邭����
            var frame = s.button.transform.Find("Frame").gameObject;
            var front = s.button.transform.Find("Front").gameObject;

            if (s.shape == selectingShape)
            {
                frame.SetActive(true);
                front.SetActive(false);
            }
            else
            {
                frame.SetActive(false);
                front.SetActive(true);
            }
        }

        SelectingShapeDisp();
    }

    /// <summary>
    /// �I�𒆂̐}�`��\��
    /// </summary>
    void SelectingShapeDisp()
    {
        for (int i = 0; i < shapesUI.Length; i++)
        {
            if (selectingShape == shapesUI[i].shape)
            {
                selectingShapeImage.sprite = shapesUI[i].sprite;
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
                playerInputData[z, x] = selectButtons[x, z].InputNum;
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

        // �𓚂ɕۑ�
        playerAnswer[(int)buttonPos.x, nextYPos, (int)buttonPos.y] = selectingShape;
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

    /// <summary>
    /// �S�����m�F�E�B���h�E�̕\��/��\��
    /// </summary>
    /// <param name="_isDisp">true�ŕ\��</param>
    public void ResetConfirmWindow(bool _isDisp)
    {
        confirmWindow.SetActive(_isDisp);
    }

    /// <summary>
    /// �S����
    /// </summary>
    public void DataReset()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerInputData[x, z] = 0;
                    selectButtons[x, z].NumReset();
                }
            }
        }

        ModeEnd();
    }
}
