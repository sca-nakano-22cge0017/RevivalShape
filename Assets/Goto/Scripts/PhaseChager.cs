using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseChager : MonoBehaviour
{
    private Text timerText;
    public StageController stageController;
    public TimeManager timeManager;
    [SerializeField] private Toggle[] toggles = null;
    private StageController sc;

    [SerializeField]
    private Toggle toggleButton1;
    [SerializeField]
    private Toggle toggleButton2;
    [SerializeField]
    private Toggle toggleButton3;

    void Start()
    {
        timerText = GetComponentInChildren<Text>();
        sc = GetComponent<StageController>();
        OnSelect();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnSelect()
    {
        if (toggles[0].isOn)
        {
            //sc.ToSelectPhase();
            Debug.Log("選択フェーズ");
            toggleButton3.interactable = false;
        }
    }
    public void Oncheck()
    {
        if (toggles[1].isOn)// && sc.phase != StageController.PHASE.PLAY)
        {
            //sc.ToCheckPhase();
            Debug.Log("確認フェーズ");
            toggleButton3.interactable = false;

        }
    }
    public void OnPlay()
    {
        //100%未満だったらselectに戻す
        if (toggles[2].isOn)
        {
            //sc.ToPlayPhase();
            Debug.Log("実行フェーズ");
            toggleButton1.interactable = false;
            toggleButton2.interactable = false;
            toggleButton3.interactable = true;
            // if (stageController.IsClear)
            // {
            //    
            // }
            // else
            //{
            //timeManager.TimeStop();
            //sc.ToCheckPhase();
            //  }
            timeManager.TimeStop();
        
        }
    }
}
