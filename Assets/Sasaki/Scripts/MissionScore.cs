using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionScore : MonoBehaviour
{
    //���̃~�b�V���� -> �v���C���ʁ@
    float time = 100f;
    int reConfirmation = 4;
    int missCount = 3;


    [SerializeField,Header("�~�b�V�����̕b��")] float[] missionTime;
    float judgeTime;
    [SerializeField, Header("�~�b�V�����Ċm�F�̉�")] int[] missionConfirmation;
    int judgeConfirmation;
    [SerializeField, Header("�~�b�V�����~�X�̉�")] int[] missionMiss;
    int judgeMiss;

    int scoreCount; //���̐�
    string selectStage;

    public static int totalScore = 0;

    // �ǉ�
    private string stageName;
    [SerializeField] private StageController stageController;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Text timeText;
    [SerializeField] private Text reconfirmationText;
    [SerializeField] private Text missText;
    [SerializeField] private Text[] missionText;
    [SerializeField] private Image[] icons;
    [SerializeField] private Text[] missionText_menu;
    [SerializeField] private Image[] icons_menu;
    [SerializeField] private Sprite[] icons_sp;
    private bool[] isMissionClear = new bool[3];
    public bool[] IsMissionClear { get { return isMissionClear; } private set { isMissionClear = value; } }

    // Start is called before the first frame update
    void Start()
    {
        selectStageCheck();

        // �ǉ� �~�b�V�����N���A�A�C�R���̔�\��
        foreach(var i in icons)
        {
            i.enabled = false;
        }

        for (int i = 0; i < 3; i++)
        {
            isMissionClear[i] = false;
        }

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage;
        else stageName = stageController.StageName;

        MenuMissionSetting();
    }

    // Update is called once per frame
    void Update()
    {
        //���U���g��ʂ��\�����ꂽ��
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    clearCheck();
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SceneManager.LoadScene("SelectScene");
        //}
    }

    //�X�e�[�W���Ƃ̃~�b�V���������𔻒�
    void selectStageCheck()
    {
        if(SelectButton.SelectStage != null) // null�`�F�b�N
            selectStage = SelectButton.SelectStage;
        else selectStage = stageController.StageName;

        //Stage�̕������T��
        if (selectStage.Contains("Stage"))
        {
            //Stage���󔒂ɂ���
            string stageName = selectStage.Replace("Stage","");
            //int�Ƃ��Ď����Ă���
            if (int.TryParse(stageName, out int s))
            {
                int sc = (s - 1) / 10;
                judgeTime = missionTime[sc];
                judgeConfirmation = missionConfirmation[sc];
                judgeMiss = missionMiss[sc];
                //Debug.Log(sc);

                // �ǉ� �~�b�V�������e�̕\��
                missionText[0].text = "�v���C����" + judgeTime.ToString() + "�b�ȓ�";
                missionText[1].text = "�m�F��" + judgeConfirmation.ToString() + "��ȓ�";
                missionText[2].text = "�~�X��" + judgeMiss.ToString() + "��ȓ�";

                missionText_menu[0].text = "�v���C����" + judgeTime.ToString() + "�b�ȓ�";
                missionText_menu[1].text = "�m�F��" + judgeConfirmation.ToString() + "��ȓ�";
                missionText_menu[2].text = "�~�X��" + judgeMiss.ToString() + "��ȓ�";
            }
        }

        if(selectStage.ToLower() == "tutorial")
        {
            judgeTime = missionTime[0];
            judgeConfirmation = missionConfirmation[0];
            judgeMiss = missionMiss[0];

            // �ǉ� �~�b�V�������e�̕\��
            missionText[0].text = "�v���C����" + judgeTime.ToString() + "�b�ȓ�";
            missionText[1].text = "�m�F��" + judgeConfirmation.ToString() + "��ȓ�";
            missionText[2].text = "�~�X��" + judgeMiss.ToString() + "��ȓ�";

            missionText_menu[0].text = "�v���C����" + judgeTime.ToString() + "�b�ȓ�";
            missionText_menu[1].text = "�m�F��" + judgeConfirmation.ToString() + "��ȓ�";
            missionText_menu[2].text = "�~�X��" + judgeMiss.ToString() + "��ȓ�";
        }
    }

    //�X�R�A�̃J�E���g
    void clearCheck()
    {
        //���������b��
        //float rt = time - judgeTime;
        float rt = judgeTime - time; // �c�莞�Ԃ�0��������������ɕύX
        if (rt > 0)
        {
            scoreCount++;
            isMissionClear[0] = true;
        }
        else isMissionClear[0] = false;
        //�Ċm�F�̉�
        int rc= reConfirmation - judgeConfirmation;
        if (rc < 0) // �`��ȓ��Ȃ̂�=���폜
        {
            scoreCount++;
            isMissionClear[1] = true;
        }
        else isMissionClear[1] = false;
        //���s������
        int cm = missCount - judgeMiss;
        if (cm < 0)
        {
            //Debug.Log("test");
            scoreCount++;
            isMissionClear[2] = true;
        }
        else isMissionClear[2] = false;

        totalScore += scoreCount;
        //Debug.Log(totalScore);

        // �f�[�^�ۑ�
        GameManager.StageDataUpdate(stageName, true, isMissionClear);
    }

    // �ǉ��@UI�̕\���E���l�̓���
    public void ResultDisp()
    {
        time = Mathf.FloorToInt(timeManager.TotalTime);
        //Debug.Log(time);
        reConfirmation = stageController.Reconfirmation;
        missCount = stageController.Miss;

        int min = Mathf.FloorToInt(time / 60.0f);
        //Debug.Log(min);
        int sec = Mathf.FloorToInt(time - (min * 60.0f));
        timeText.text = min.ToString("d2") + ":" + sec.ToString("d2");
        reconfirmationText.text = reConfirmation.ToString();
        missText.text = missCount.ToString();

        clearCheck();
    }

    void MenuMissionSetting()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!stageName.Contains("Stage") && !stageName.Contains("Tutorial"))
            {
                missionText_menu[i].enabled = false;
                icons_menu[i].enabled = false;
            }

            if (GameManager.GetStageData(stageName) != null)
                icons_menu[i].sprite = GameManager.GetStageData(stageName).IsMissionClear[i] ? icons_sp[1] : icons_sp[0];
        }
    }
}
