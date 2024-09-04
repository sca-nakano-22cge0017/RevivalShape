using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
    [SerializeField] private CountUpAnimation countUpAnimation;

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

    public bool isInAnimation = false; // �A�j���[�V��������

    // SE
    private SoundManager sm;

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        stageController = GameObject.FindObjectOfType<StageController>();
        tutorial = GameObject.FindObjectOfType<Tutorial>();
        sm = FindObjectOfType<SoundManager>();

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

                selectPhase.IsLongTap = true;
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

    /// <summary>
    /// �m�F���[�h���̃n�C���C�g������
    /// </summary>
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
        if (!selectPhase.IsLongTap && selectPhase.CanSwipInput) CountUp();
    }

    public void PointerUp()
    {
        CountEnd();
        selectPhase.CanSwipInput = false;
    }

    public void PointerExit()
    {
        if (!selectPhase.IsLongTap) CountEnd();
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

        selectPhase.IsLongTap = false;
    }

    void NumberChange()
    {
        // �A�j���[�V�������Ȃ�X�L�b�v
        if (isInAnimation) return;

        if (!isEraserMode && inputNum < max)
        {
            StartCoroutine(CountAnimation());
            SEPlay();
        }
        else if (isEraserMode && inputNum > 0)
        {
            StartCoroutine(CountAnimation());
            SEPlay();
        }
    }

    /// <summary>
    /// ���o����
    /// </summary>
    /// <param name="isCountUp">true�̂Ƃ������@false�̂Ƃ�����</param>
    /// <returns></returns>
    IEnumerator CountAnimation()
    {
        isInAnimation = true;
        
        int add = isEraserMode ? -1 : 1;
        string animBoolName = isEraserMode ? "CountDown" : "CountUp";

        inputNum += add;

        if (!isEraserMode) selectPhase.ShapeInput(Position); // �}�`�ǉ�
        else selectPhase.ShapeDelete(Position); // �}�`�폜

        nextText.text = inputNum.ToString();

        textMoveAnim.SetTrigger(animBoolName);

        // �A�j���[�V�����I����A�e�L�X�g���X�V
        yield return new WaitUntil(() => countUpAnimation.isAnimationEnd);

        currentText.text = inputNum.ToString();

        countUpAnimation.isAnimationEnd = false;
        isInAnimation = false;
    }

    /// <summary>
    /// ���͐���������
    /// </summary>
    public void NumReset()
    {
        inputNum = 0;
        currentText.text = inputNum.ToString();
        nextText.text = inputNum.ToString();
    }

    void SEPlay()
    {
        if (sm != null)
        {
            sm.SEPlay3();
        }
    }
}
