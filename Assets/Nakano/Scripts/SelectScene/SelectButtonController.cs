using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �I����ʂ̃{�^������ UI�\��
/// </summary>
public class SelectButtonController : MonoBehaviour
{
    // ����
    //�u�X�e�[�W�ԍ��@�v�F10�X�e�[�W���̔ԍ��B0�̂Ƃ��̓X�e�[�W1�`10, 1�̂Ƃ��̓X�e�[�W11�`20�B
    //�u�X�e�[�W�ԍ��A�v�F�ʏ�X�e�[�W�ɐU���Ă���ԍ��B�X�e�[�W1�`100�B

    GameManager gameManager = null;
    private int stageAmount = 0;

    Dictionary<int, bool> stageRelease = new(); // ������
    Dictionary<int, bool> extraRelease = new();

    [SerializeField] private GameObject tutorialButton;

    [SerializeField, Header("�X�e�[�W�I����ʇ@")] GameObject firstSelectPanel;
    [SerializeField, Header("�X�e�[�W�I����ʇ@�̃{�^��")] private Button[] buttons_FirstSelect;
    
    [SerializeField, Header("�X�e�[�W�I����ʇA")] private SelectButton selectButton;
    [SerializeField, Header("�X�e�[�W�I����ʇA�̃{�^��")] private Button[] buttons_SecondSelect;
    [SerializeField] private Sprite[] missionIcons_sp;
    [SerializeField, Header("Content")] private RectTransform secondContent = null;

    [SerializeField, Header("�G�L�X�g���{�^��������������Content�̏c��")] private float contentHeight = 400.0f;
    [SerializeField] private int width;
    [SerializeField] private int maxHeight;

    bool loaded = false;

    public static int selectNumber = 0; // �I�������X�e�[�W�ԍ��@

    void Start()
    {
        if (GameObject.FindObjectOfType<GameManager>() != null)
        {
            gameManager = GameObject.FindObjectOfType<GameManager>();
            stageAmount = gameManager.StageAmount;
        }

        // �X�e�[�W�̉����Ԃ��������@��ԍŏ���10�X�e�[�W�͉���ς݂ɂ���
        // i = �X�e�[�W�ԍ��@
        for (int i = 0; i < stageAmount / 10; i++)
        {
            if (i == 0) stageRelease.Add(i, true);
            else stageRelease.Add(i, false);
            extraRelease.Add(i, false);
        }

        loaded = false;
    }

    private void Update()
    {
        if(gameManager != null)
        {
            // �Z�[�u�f�[�^�̓ǂݍ��݊�����A��x�����Ăяo��
            if (gameManager.DidLoad && !loaded)
            {
                loaded = true;

                // UI�̐ݒ�
                FirstButtonsSetting();

                if (SceneName.GetLastSceneName() == "MainScene")
                {
                    if(selectNumber > 0)
                        FirstSelect(selectNumber);
                }
            }
        }
    }

    /// <summary>
    /// �X�e�[�W�I����ʇ@�̐ݒ�
    /// </summary>
    void FirstButtonsSetting()
    {
        firstSelectPanel.SetActive(true);

        for (int i = 0; i < stageAmount / 10; i++)
        {
            // �e�{�^���̐ݒ�
            var fsb = buttons_FirstSelect[i].GetComponent<FirstSelectButton>();
            fsb.sbController = this;
            fsb.num = i;

            Release(i);

            // ����ς݂̃X�e�[�W�̓{�^���������ł���悤�ɂ���
            buttons_FirstSelect[i].interactable = stageRelease[i];
        }

        // Tutorial
        if (gameManager.GetStageData("Tutorial") != null)
        {
            StageData data = gameManager.GetStageData("Tutorial");
            var imageParent = tutorialButton.transform.Find("Mission");
            MissionIconDisp(imageParent, data);
        }

        var ssb = tutorialButton.GetComponent<SecondSelectButton>();
        ssb.selectButton = selectButton;
        ssb.stageName = "Tutorial";
    }

    /// <summary>
    /// �X�e�[�W�I����ʇ@�ɖ߂�
    /// </summary>
    public void Back()
    {
        firstSelectPanel.SetActive(true);
    }

    /// <summary>
    /// �X�e�[�W�I����ʇ@�̃{�^���������̏���
    /// </summary>
    /// <param name="num">�������{�^���̔ԍ�</param>
    public void FirstSelect(int num)
    {
        selectNumber = num;
        firstSelectPanel.SetActive(false);

        SecondButtonsSetting(num);
    }

    /// <summary>
    /// �X�e�[�W�I����ʇA�̐ݒ�
    /// </summary>
    /// <param name="num">�I�������X�e�[�W�ԍ��@</param>
    void SecondButtonsSetting(int num)
    {
        int undispButton = 0; // ��\���ɂ���{�^���̐�

        for (int i = 0; i < buttons_SecondSelect.Length; i++)
        {
            string stageName = "";
            
            if (i == buttons_SecondSelect.Length - 1)
            {
                stageName = "Extra" + (num + 1).ToString();
                buttons_SecondSelect[i].interactable = extraRelease[num];

                // ���łł�11�`20�X�e�[�W��Extra�͖���
                if(num == 1)
                {
                    buttons_SecondSelect[i].gameObject.SetActive(false);
                    undispButton++;
                }
            }
            else
            {
                stageName = "Stage" + (num * 10 + i + 1).ToString();
            }

            // Text�\���ύX
            var childText = buttons_SecondSelect[i].transform.GetComponentInChildren<Text>();
            childText.text = stageName.ToUpper();

            // Extra�ATutorial�ȊO�̓~�b�V�����N���A�̃A�C�R���\����ύX
            if(stageName.Contains("Stage"))
            {
                if (gameManager.GetStageData(stageName) != null)
                {
                    StageData data = gameManager.GetStageData(stageName);
                    var imageParent = buttons_SecondSelect[i].transform.Find("Mission");
                    MissionIconDisp(imageParent, data);
                }
            }
            
            var ssb = buttons_SecondSelect[i].GetComponent<SecondSelectButton>();
            ssb.selectButton = selectButton;
            ssb.stageName = stageName;
        }

        // Content�T�C�Y����
        var height = maxHeight - (undispButton * contentHeight);
        secondContent.sizeDelta = new Vector2(width, height);
    }

    public void TutorialSelect()
    {
        selectNumber = -1;
    }

    void MissionIconDisp(Transform _parent, StageData _data)
    {
        for (int i = 0; i < 3; i++)
        {
            int spNum = _data.IsMissionClear[i] ? 1 : 0;
            _parent.GetChild(i).GetComponent<Image>().sprite = missionIcons_sp[spNum];
        }
    }

    /// <summary>
    /// �X�e�[�W�����Ԃ̕ύX
    /// </summary>
    /// <param name="num">�I�������X�e�[�W�ԍ��@</param>
    void Release(int num)
    {
        int starsAmount = 0;

        // �w�肵��10�X�e�[�W�̐��l�������m�F
        for(int i = 1; i <= 10; i++)
        {
            string stageName = "Stage" + (i + num).ToString();

            if(gameManager.GetStageData(stageName) != null)
                starsAmount += gameManager.GetStageData(stageName).GotStar;
        }

        // ����10�X�e�[�W�����
        if (starsAmount >= 25)
        {
            if(num < stageAmount) stageRelease[num + 1] = true;
        }
        // �G�N�X�g���X�e�[�W�����
        if (starsAmount >= 30)
        {
            extraRelease[num] = true;
        }
    }
}
