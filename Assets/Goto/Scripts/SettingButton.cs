using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SettingButton : MonoBehaviour
{
    public FadeButtonA fadeButton;
    public TimeManager timeManager;
    
    [SerializeField] public GameObject[] Panels =null;
    [SerializeField] public GameObject panel;


    public void MenuOnPush()
    {
       
         Panels[2].SetActive(true);
        panel.SetActive(true);
        timeManager.OnStop();
       // fadeButton.FadeOut();
    }

    public void MenuOffPush()
    {
        Panels[2].SetActive(false);

       // timeManager.OnStart(); //É^ÉCÉÄêiÇ‹Ç»Ç¢
        //fadeButton.FadeIn();
    }
    public void MenuSoundOn()
    {
        Panels[2].SetActive(false);
        Panels[1].SetActive(true);
        timeManager.OnStop();
       
    }
    public void MenuSoundOff()
    {
        Panels[2].SetActive(true);
        Panels[1].SetActive(false);
        timeManager.OnStop();
        
    }
    public void SelectScene()
    {
        Panels[2].SetActive(false);
        Panels[1].SetActive(false);
        //SceneManager.LoadScene("SelectScene");
        SceneLoader.Load("SelectScene");
    }
}
