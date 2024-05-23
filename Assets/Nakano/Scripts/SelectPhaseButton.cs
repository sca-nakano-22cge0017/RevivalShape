using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �I���t�F�[�Y�Ŏg�p����{�^��
/// </summary>
public class SelectPhaseButton : MonoBehaviour
{
    SelectPhase selectPhase;

    /// <summary>
    /// �e�{�^���ɓ��͂��ꂽ��
    /// </summary>
    public int InputNum { get; set; } = 0;

    /// <summary>
    /// �{�^���̈ʒu
    /// </summary>
    public Vector2 Position { get; set; } = new Vector2(0, 0);

    Text thisText;

    /// <summary>
    /// ���͂ł���ő�l
    /// </summary>
    public int Input_max { get; set; } = 10;

    // ������
    bool isCountForLongTap = false; // �����O�^�b�v�p�̃J�E���g���J�n����t���O
    bool isLongTap = false;         // �����������ǂ���
    [SerializeField, Header("�������� ���l��1�オ��܂ł̎���")] float interval = 0.5f;
    [SerializeField, Header("�����������܂ł̎���")] float longTapTime = 2.0f;
    float tapStartTime = 0;

    // �������[�h
    bool isEraserMode = false; // �������[�h���ǂ���

    // �m�F���[�h
    bool isCheckMode = false; // �m�F���[�h���ǂ���
    [SerializeField] Image flame; // �m�F�J�������[�h���̔���

    public bool IsCheck{ get; set; } = false; // �m�F����}�X�̃{�^����
    
    private void Start()
    {
        // �{�^���̎q�I�u�W�F�N�g��Text���擾
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();

        selectPhase = FindObjectOfType<SelectPhase>();

        flame.enabled = false;
    }

    private void Update()
    {
        // �\���ύX
        thisText.text = InputNum.ToString();

        isEraserMode = selectPhase.IsEraser;
        isCheckMode = selectPhase.IsCheck;

        // ���������m�F�J�n
        if (isCountForLongTap)
        {
            // �J�E���g�J�n
            tapStartTime += Time.deltaTime;

            // ��莞�Ԍo�߂����璷�����Ƃ��ď���
            if(tapStartTime >= longTapTime)
            {
                isLongTap = true;
                isCountForLongTap = false;
                tapStartTime = 0;
            }
        }

        // �������Ȃ�
        if (isLongTap)
        {
            tapStartTime += Time.deltaTime;

            // ��莞�Ԍo�ߖ��ɑ�������
            if(interval <= tapStartTime)
            {
                tapStartTime = 0;

                if(!isEraserMode) InputNum++;
                else InputNum--;
            }
        }

        if (!isCheckMode)
        {
            IsCheck = false;
            flame.enabled = false;
        }

        if (IsCheck)
        {
            var rt = GetComponent<RectTransform>();
            flame.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
            flame.enabled = true;
        }
        else flame.enabled = false;
    }

    /// <summary>
    /// �{�^������������J�E���g�𑝂₷
    /// </summary>
    public void CountUp()
    {
        // �m�F���[�h�̂Ƃ��ɉ����ꂽ��{�^���ɉ������}�X�ɔz�u���ꂽ�}�`��\������
        if (isCheckMode)
        {
            IsCheck = true;
            selectPhase.CheckWindowDisp();
            return;
        }

        if (!isEraserMode && InputNum < Input_max)
        {
            InputNum++;
            selectPhase.ShapeInput(Position);  // �}�`�ǉ�
        }
        else if (isEraserMode && InputNum > 0)
        {
            InputNum--;
            selectPhase.ShapeDelete(Position); // �}�`����
        }

        isCountForLongTap = true;
        tapStartTime = 0;
    }

    /// <summary>
    /// �{�^�������𗣂������̏���
    /// </summary>
    public void CountEnd()
    {
        isLongTap = false;
        isCountForLongTap = false;
        tapStartTime = 0;
    }
}
