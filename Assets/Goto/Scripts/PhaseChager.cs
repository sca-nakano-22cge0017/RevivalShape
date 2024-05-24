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
    [SerializeField] private GameObject openPanel;
    [SerializeField] private GameObject blockSelectPanel;
    [SerializeField] private GameObject checkPanel;


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
          //  stageController.ToCheckPhase();
            Debug.Log("�m�F�t�F�[�Y");

            //���s�t�F�[�Y��toggle�������Ȃ��悤�ɂ���
            toggles[1].interactable = true;
            toggles[2].interactable = false;

            //�u���b�N�I����ʏ���
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(true);
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
           // stageController.ToSelectPhase();
            Debug.Log("�I���t�F�[�Y");

            //���s�t�F�[�Y��toggle�������Ȃ��悤�ɂ���
            toggles[0].interactable = true;
            toggles[2].interactable = true;

            //�u���b�N�I����ʂ��o��
            openPanel.SetActive(true);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(false);


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
           // stageController.ToPlayPhase();
            Debug.Log("���s�t�F�[�Y");

            //���s�t�F�[�Y���ړ����Ȃ�����
            toggles[0].interactable = false;
            toggles[1].interactable = false;

            // ���s�t�F�[�Y���͎��Ԃ��~�߂�
            timeManager.TimeStop();

            //�u���b�N�I����ʏ���
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
        }
        else
        {
            //���s�I���Ɏ��Ԃ�i�߂�
            timeManager.TimeStop();
        }
    }
    
}

