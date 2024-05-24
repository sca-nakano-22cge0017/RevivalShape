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
    /// 確認フェーズが押されたら
    /// 押されたタイミングの処理、表示の切り替え
    /// </summary>
    public void Oncheck()
    {
        if (toggles[0].isOn)// && sc.phase != StageController.PHASE.PLAY)
        {
          //  stageController.ToCheckPhase();
            Debug.Log("確認フェーズ");

            //実行フェーズのtoggleを押せないようにする
            toggles[1].interactable = true;
            toggles[2].interactable = false;

            //ブロック選択画面消す
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(true);
        }
    }
    /// <summary>
    /// 選択フェーズが押されたら
    /// 押されたタイミングの処理、表示の切り替え
    /// </summary>
    public void OnSelect()
    {
        if (toggles[1].isOn)
        {
           // stageController.ToSelectPhase();
            Debug.Log("選択フェーズ");

            //実行フェーズのtoggleを押せないようにする
            toggles[0].interactable = true;
            toggles[2].interactable = true;

            //ブロック選択画面を出す
            openPanel.SetActive(true);
            blockSelectPanel.SetActive(false);
            checkPanel.SetActive(false);


        }
    }
    /// <summary>
    /// 実行フェーズが押されたら
    /// 押されたタイミングの処理、表示の切り替え
    /// </summary>
    public void OnPlay()
    {
        //100%未満だったらselectに戻す
        if (toggles[2].isOn)
        {
           // stageController.ToPlayPhase();
            Debug.Log("実行フェーズ");

            //実行フェーズ中移動しなくする
            toggles[0].interactable = false;
            toggles[1].interactable = false;

            // 実行フェーズ中は時間を止める
            timeManager.TimeStop();

            //ブロック選択画面消す
            openPanel.SetActive(false);
            blockSelectPanel.SetActive(false);
        }
        else
        {
            //実行終わりに時間を進める
            timeManager.TimeStop();
        }
    }
    
}

