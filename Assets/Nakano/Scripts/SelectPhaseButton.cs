using UnityEngine;
using UnityEngine.UI;

public class SelectPhaseButton : MonoBehaviour
{
    SelectPhase selectPhase;

    int inputNum = 0;

    /// <summary>
    /// �e�{�^���ɓ��͂��ꂽ��
    /// </summary>
    public int InputNum
    {
        get { return inputNum; }
        set { inputNum = value; }
    }

    Text thisText;

    const int input_max = 10; // ���͂ł���ő�l

    // ������
    bool isCountForLongTap = false; // �����O�^�b�v�p�̃J�E���g���J�n����t���O
    bool isLongTap = false; // �����������ǂ���
    [SerializeField, Header("�������� ���l��1�オ��܂ł̎���")] float interval = 0.5f;
    [SerializeField, Header("�����������܂ł̎���")] float longTapTime = 2.0f;
    float tapStartTime = 0;

    // �������[�h
    bool isEraserMode = false; // �������[�h���ǂ���

    // �m�F���[�h
    bool isCheckMode = false; // �m�F���[�h���ǂ���
    public bool isCheck{ get; set; } = false; // �m�F����}�X�̃{�^����
    
    private void Start()
    {
        // �{�^���̎q�I�u�W�F�N�g��Text���擾
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();

        selectPhase = FindObjectOfType<SelectPhase>();
    }

    private void Update()
    {
        thisText.text = inputNum.ToString(); // �\���ύX

        isEraserMode = selectPhase.IsEraser;
        isCheckMode = selectPhase.IsCheck;

        // ������
        if (isCountForLongTap)
        {
            tapStartTime += Time.deltaTime;

            if(tapStartTime >= longTapTime)
            {
                isLongTap = true;
                isCountForLongTap = false;
                tapStartTime = 0;
            }
        }

        if (isLongTap)
        {
            tapStartTime += Time.deltaTime;
            if(interval <= tapStartTime)
            {
                tapStartTime = 0;

                if(!isEraserMode) inputNum++;
                else inputNum--;
            }
        }

        if (inputNum > input_max) inputNum = input_max;
        if (inputNum <= 0) inputNum = 0;

        if(!isCheckMode) isCheck = false;
    }

    /// <summary>
    /// �{�^������������J�E���g�𑝂₷
    /// </summary>
    public void CountUp()
    {
        // �m�F���[�h�̂Ƃ��ɉ����ꂽ��{�^���ɉ������}�X�ɔz�u���ꂽ�}�`��\������
        if (isCheckMode)
        {
            isCheck = true;
            selectPhase.CheckWindowDisp();
            return;
        }

        if (!isEraserMode) inputNum++;
        else inputNum--;

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
