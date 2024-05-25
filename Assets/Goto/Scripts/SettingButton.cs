using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingButton : MonoBehaviour
{
    [SerializeField] private GameObject[] Panels =null;


    public void MenuOnPush()
    {
        Panels[2].SetActive(true);
      
        Time.timeScale = 0;
    }

    public void MenuOffPush()
    {
        Panels[2].SetActive(false);
       
        Time.timeScale = 1;
    }
    public void MenuSoundOn()
    {
        Panels[2].SetActive(false);
        Panels[1].SetActive(true);
        Time.timeScale = 0;
    }
    public void MenuSoundOff()
    {
        Panels[2].SetActive(true);
        Panels[1].SetActive(false);
        Time.timeScale = 0;
    }
}
