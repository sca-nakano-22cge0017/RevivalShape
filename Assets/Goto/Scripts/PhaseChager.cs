using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseChager : MonoBehaviour
{
    private Text timerText;
    public StageController stageController;
    public TimeManager timeManager;
    public BlockSelectButton blockSelectButton;
    [SerializeField] private Toggle[] toggles = null;
    /// <summary>
    /// �I���t�F�[�Y�̃p�l��
    /// </summary>
    [SerializeField] private GameObject openPanel;
    /// <summary>
    /// �I���t�F�[�Y�̃u���b�N�I���p�l��
    /// </summary>
    [SerializeField] private GameObject blockSelectPanel;
    /// <summary>
    /// �m�F�t�F�[�Y�̃p�l��
    /// </summary>
    [SerializeField] private GameObject checkPanel;
    /// <summary>
    /// �ݒ��ʂ̃p�l��
    /// </summary>
    [SerializeField] private GameObject settingPanel;


    void Start()
    {
        timerText = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
    //    if (stageController.phase == StageController.PHASE.PLAY)
    //    {
           
    //        toggleButton1.interactable = true;
    //        toggleButton3.interactable = false;
    //    }

    //    if (stageController.phase == StageController.PHASE.SELECT)
    //    {
    //        toggleButton1.interactable = true;
    //        toggleButton3.interactable = false;
           
    //    }

    //    if (stageController.phase == StageController.PHASE.SELECT)
    //    {
    //        toggles[2].isOn=true;
    //        toggleButton1.interactable = false;
    //        toggleButton2.interactable = false;
    //        toggleButton3.interactable = true;
    //        timeManager.TimeStop();
    //    }
    }
    /// <summary>
    /// �m�F�t�F�[�Y�������ꂽ��
    /// �����ꂽ�^�C�~���O�̏����A�\���̐؂�ւ�
    /// </summary>
    public void Oncheck()
    {
     
        if (toggles[0].isOn)// && sc.phase != StageController.PHASE.PLAY)
        {
            
            if(stageController) stageController.ToCheckPhase();
            //Debug.Log("�m�F�t�F�[�Y");

            //���s�t�F�[�Y��toggle�������Ȃ��悤�ɂ���
            toggles[1].interactable = true;
            toggles[2].interactable = false;

            //�u���b�N�I����ʏ���
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(true);
            //settingPanel.SetActive(true);

            // timeManager.OnStart();//test
        }
    }
    /// <summary>
    /// �I���t�F�[�Y�������ꂽ��
    /// �����ꂽ�^�C�~���O�̏����A�\���̐؂�ւ�
    /// </summary>
    public void OnSelect()
    {
        if (toggles[1].isOn)
        {
            if(stageController) stageController.ToSelectPhase();
            //Debug.Log("�I���t�F�[�Y");

            //���s�t�F�[�Y��toggle�������Ȃ��悤�ɂ���
            toggles[0].interactable = true;
            toggles[2].interactable = false;

            //�u���b�N�I����ʂ��o��
            openPanel.SetActive(true);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(false);
            //settingPanel.SetActive(true);

            //timeManager.OnStop();//test

        }
    }
    /// <summary>
    /// ���s�t�F�[�Y�������ꂽ��
    /// �����ꂽ�^�C�~���O�̏����A�\���̐؂�ւ�
    /// </summary>
    public void OnPlay()
    {
        //100%������������select�ɖ߂�
        if (toggles[2].isOn)
        {

            if (stageController) stageController.ToPlayPhase();
            //Debug.Log("���s�t�F�[�Y");

            //���s�t�F�[�Y���ړ����Ȃ�����
            toggles[0].interactable = false;
            toggles[1].interactable = false;

            // ���s�t�F�[�Y���͎��Ԃ��~�߂�
            timeManager.OnStop();
            

            //�u���b�N�I����ʏ���
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
            //settingPanel.SetActive(false);
        }
        else
        {
            //���s�I���Ɏ��Ԃ�i�߂�
            timeManager.OnStart();
        }
    }
    
}

