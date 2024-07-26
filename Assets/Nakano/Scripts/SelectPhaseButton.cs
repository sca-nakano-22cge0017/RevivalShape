using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// �I���t�F�[�Y�Ŏg�p����{�^��
/// </summary>
public class SelectPhaseButton : MonoBehaviour
{
    private StageController stageController;
    private Tutorial tutorial;

    [SerializeField] private Animator textMoveAnim;
    [SerializeField] private Text currentText;
    [SerializeField] private Text nextText;

    public SelectPhase selectPhase{ get; set;}

    private int inputNum = 0;
    /// <summary>
    /// �e�{�^���ɓ��͂��ꂽ��
    /// </summary>
    public int InputNum { get{ return inputNum; } set{ inputNum = value; } }

    private Vector2 position = new Vector2(0, 0);
    /// <summary>
    /// �{�^���̈ʒu
    /// </summary>
    public Vector2 Position { get { return position; } set { position = value;  } }

    private int max = 10;
    /// <summary>
    /// ���͂ł���ő�l
    /// </summary>
    public int InputMax { get{ return max; } set { max = value; } }

    // ������
    private bool isCountForLongTap = false; // �����O�^�b�v�p�̃J�E���g���J�n����t���O
    private bool isLongTap = false;         // �����������ǂ���
    [SerializeField, Header("�������� ���l��1�オ��܂ł̎���")] private float interval = 0.5f;
    [SerializeField, Header("�����������܂ł̎���")] private float longTapTime = 2.0f;
    private float tapStartTime = 0;

    // �������[�h
    private bool isEraserMode = false; // �������[�h���ǂ���

    // �m�F���[�h
    private bool isCheckMode = false; // �m�F���[�h���ǂ���
    [SerializeField] private Image flame; // �m�F�J�������[�h���̔���

    private bool isCheck = false;
    public bool IsCheck{ get { return isCheck; } set { isCheck = value; } } // �m�F����}�X�̃{�^����

    private bool isInAnimation = false; // �A�j���[�V��������

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        stageController = GameObject.FindObjectOfType<StageController>();
        tutorial = GameObject.FindObjectOfType<Tutorial>();

        var rt = GetComponent<RectTransform>();
        flame.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
        flame.enabled = false;
    }

    private void Update()
    {
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

                NumberChange();
            }
        }

        if (!isCheckMode)
        {
            IsCheck = false;
            flame.enabled = false;
        }
    }

    public void ShapeCheckEnd()
    {
        flame.enabled = false;
    }

    public void PointerDown()
    {
        CountUp();
        if(!isCheckMode) selectPhase.CanSwipInput = true;
    }

    public void PointerEnter()
    {
        if(selectPhase.CanSwipInput) CountUp();
    }

    public void PointerUp()
    {
        CountEnd();
        selectPhase.CanSwipInput = false;
    }

    public void PointerExit()
    {
        CountEnd();
    }

    /// <summary>
    /// �{�^������������J�E���g�𑝂₷
    /// </summary>
    void CountUp()
    {
        if(stageController.IsTutorial)
        {
            tutorial.ToSelectC = true;
        }

        // �m�F���[�h�̂Ƃ��ɉ����ꂽ��{�^���ɉ������}�X�ɔz�u���ꂽ�}�`��\������
        if (isCheckMode)
        {
            IsCheck = true;
            selectPhase.CheckWindowDisp(position);
            flame.enabled = true;
            return;
        }
        
        NumberChange();

        if(selectPhase.CanSwipInput) return;

        isCountForLongTap = true;
        tapStartTime = 0;
    }

    /// <summary>
    /// �{�^�������𗣂������̏���
    /// </summary>
    void CountEnd()
    {
        isLongTap = false;
        isCountForLongTap = false;
        tapStartTime = 0;
    }

    void NumberChange()
    {
        // �A�j���[�V�������Ȃ�X�L�b�v
        if(isInAnimation) return;

        if (!isEraserMode && inputNum < max)
        {
            StartCoroutine(CountAnimation(true));
        }
        else if (isEraserMode && inputNum > 0)
        {
            StartCoroutine(CountAnimation(false));
        }
    }

    /// <summary>
    /// ���o����
    /// </summary>
    /// <param name="isCountUp">true�̂Ƃ������@false�̂Ƃ�����</param>
    /// <returns></returns>
    IEnumerator CountAnimation(bool isCountUp)
    {
        isInAnimation = true;

        int add = isCountUp ? 1 : -1;
        string animBoolName = isCountUp ? "CountUp" : "CountDown";

        currentText.text = inputNum.ToString();
        nextText.text = (inputNum + add).ToString();

        // �A�j���[�V�������̏ꍇ�͏I���܂őҋ@
        yield return new WaitUntil(() => textMoveAnim.GetCurrentAnimatorStateInfo(0).IsName("Default"));
        textMoveAnim.SetTrigger(animBoolName);

        inputNum += add;
        selectPhase.ShapeInput(Position);  // �}�`�ǉ�

        // �A�j���[�V�����I����A�e�L�X�g���X�V
        yield return new WaitUntil(() => textMoveAnim.GetCurrentAnimatorStateInfo(0).IsName("Default"));
        currentText.text = inputNum.ToString();

        isInAnimation = false;
    }
}
