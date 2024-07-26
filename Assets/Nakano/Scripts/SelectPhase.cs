using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace select
{
    /// <summary>
    /// �e�}�`����2D�摜�A�}�`�ύX���̃{�^��
    /// </summary>
    [System.Serializable]
    public class ShapesUI
    {
        public ShapeData.Shape shape;
        public Sprite sprite;
        public GameObject button; // �}�`�ύX�{�^��
    }
}

/// <summary>
/// �I���t�F�[�Y
/// </summary>
public class SelectPhase : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;

    [SerializeField] private GameObject buttonParent;
    [SerializeField] private GameObject buttonPrefab;

    [SerializeField, Header("�{�^���\���͈�")] private Vector2 buttonRange;

    [SerializeField] private GameObject selectPhaseUI;

    private SelectPhaseButton[,] selectButtons; // �e�{�^���̃f�[�^

    private int[,] playerInputData;
    private ShapeData.Shape selectingShape; // �I�𒆂̐}�`

    private ShapeData.Shape[] shapeType;     // �g�p�}�`�̎��

    private Vector3 mapSize;

    private ShapeData.Shape[,,] playerAnswer; // �v���C���[�̉�

    // �e�}�`����UI
    [SerializeField] private select.ShapesUI[] shapesUI;

    // �폜���[�h
    [SerializeField] private GameObject eraserModeButton;
    public bool IsEraser { get; set; } = false;

    // �m�F���[�h
    [SerializeField] private GameObject checkModeButton;
    [SerializeField] private GameObject checkModeWindow;
    [SerializeField] private GameObject stepsParent;
    [SerializeField] private GameObject stepsPrefab;
    private Image[] steps;
    public bool IsCheck { get; private set; } = false;
    private Vector2 checkPos = new Vector2(0, 0); // �m�F����}�X

    // �m�F�J�������[�h�̃E�B���h�E�������ł��邩
    private bool canCheckWindowUnDisp = false;

    // �X���C�v���Đ������㏸�������邩
    public bool CanSwipInput { get; set; } = false;

    // �e���[�h���̍��w�i
    [SerializeField] private GameObject modeBG;

    [SerializeField] private GameObject clearBG;

    [SerializeField] private PlayPhase play;

    private void Update()
    {
        if (stageController.phase != StageController.PHASE.SELECT) return;

        if (IsEraser || IsCheck)
        {
            // �{�^���̊O���������烂�[�h�I��
            if (Input.touchCount >= 1)
            {
                Touch t = Input.GetTouch(0);

                Vector2 min = new Vector2(Screen.width / 2 - buttonRange.x / 2, Screen.height / 2 - buttonRange.y / 2);
                Vector2 max = new Vector2(Screen.width - min.x, Screen.height - min.y);

                if (t.phase == TouchPhase.Began &&
                    !tapManager.TapOrDragRange(t.position, min, max))
                {
                    if (IsEraser) IsEraser = false;
                    if (IsCheck) IsCheck = false;
                }

                ModeUIDispSet();
            }
        }

        // ��ʃ^�b�v�Ŋm�F�J�������[�h�̃E�B���h�E�����
        if (canCheckWindowUnDisp && Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            checkModeWindow.SetActive(false);
            canCheckWindowUnDisp = false;

            selectButtons[(int)checkPos.x, (int)checkPos.y].ShapeCheckEnd();
        }
    }

    public void Initialize()
    {
        if (!stageController) return;

        // �E�B���h�E�AUI��\��
        selectPhaseUI.SetActive(false);
        checkModeWindow.SetActive(false);
        modeBG.SetActive(false);
        clearBG.SetActive(false);

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �g�p�}�`���擾
        shapeType = stageController.ShapeType;

        selectButtons = new SelectPhaseButton[(int)mapSize.x, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerInputData = new int[(int)mapSize.x, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    selectButtons[x, z] = null;
                    playerInputData[x, z] = 0;
                }
            }
        }

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
                selectButtons[x, z].InputMax = (int)mapSize.y;
                selectButtons[x, z].selectPhase = this;
            }
        }

        // �m�F�J�������[�h�̃E�B���h�E�ݒ�
        steps = new Image[(int)mapSize.y];
        stepsParent.GetComponent<RectTransform>().transform.localPosition = new Vector2(0, 900 + mapSize.y * 50);
        for (int y = 0; y < (int)mapSize.y; y++)
        {
            steps[y] = Instantiate(stepsPrefab, stepsParent.transform).GetComponent<Image>();

            float size = 200 * (1 - 1 / mapSize.y);
            steps[y].GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);

            // �������Hierarchy�̈�ԏ�ɐݒ肷�� �� �����珇�ɐ�������
            steps[y].transform.SetAsFirstSibling();
        }

        // �}�`�ύX�{�^���ݒ�
        bool firstShape = true; // ��ԍŏ��ɔz�u����}�`

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
    public void SelectPhaseStart()
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

    /// <summary>
    /// �I���t�F�[�Y�I��
    /// </summary>
    public void SelectPhaseEnd()
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

    void ModeUIDispSet()
    {
        Color clear = new Color(1, 1, 1, 0);
        Color unClear = new Color(1, 1, 1, 1);

        if (IsEraser || IsCheck)
        {
            // ���[�h���͔w�i�Â�
            modeBG.SetActive(true);
            clearBG.SetActive(true);
        }
        else
        {
            checkModeButton.GetComponent<Image>().color = clear;
            eraserModeButton.GetComponent<Image>().color = clear;

            modeBG.SetActive(false);
            clearBG.SetActive(false);
        }

        if (IsEraser)
        {
            checkModeButton.GetComponent<Image>().color = clear;
            eraserModeButton.GetComponent<Image>().color = unClear;

            checkModeButton.transform.SetAsFirstSibling();
            eraserModeButton.transform.SetAsLastSibling();
        }
        if (IsCheck)
        {
            checkModeButton.GetComponent<Image>().color = unClear;
            eraserModeButton.GetComponent<Image>().color = clear;

            eraserModeButton.transform.SetAsFirstSibling();
            checkModeButton.transform.SetAsLastSibling();
        }
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
            foreach (var ui in shapesUI)
            {
                if (ui.shape == shape)
                {
                    steps[i].sprite = ui.sprite;
                }
            }
        }

        // �E�B���h�E�̕\���Ɣ�\������u�ōs����̂ŕ\�����莞�ԑ҂��Ă���A��\���ɂł���悤�ɂ���
        StartCoroutine(stageController.DelayCoroutine(0.1f, () =>
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
}
