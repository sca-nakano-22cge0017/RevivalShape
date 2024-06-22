using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionScore : MonoBehaviour
{
    //���̃~�b�V����
    float time = 80f;
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
    // Start is called before the first frame update
    void Start()
    {
        selectStageCheck();
    }

    // Update is called once per frame
    void Update()
    {
        //���U���g��ʂ��\�����ꂽ��
        if (Input.GetKeyDown(KeyCode.A))
        {
            clearCheck();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene("SelectScene");
        }
    }

    //�X�e�[�W���Ƃ̃~�b�V���������𔻒�
    void selectStageCheck()
    {
        selectStage = SelectButton.SelectStage;

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
                Debug.Log(sc);
            }
        }
    }

    //�X�R�A�̃J�E���g
    void clearCheck()
    {
        //���������b��
        float rt = time - judgeTime;
        if (rt <= 0)
        {
            scoreCount++;
        }
        //�Ċm�F�̉�
        int rc= reConfirmation - judgeConfirmation;
        if (rc <= 0)
        {
            scoreCount++;
        }
        //���s������
        int cm = missCount - judgeMiss;
        if (cm <= 0)
        {
            scoreCount++;
        }
        totalScore += scoreCount;
        Debug.Log(totalScore);
    }
}
