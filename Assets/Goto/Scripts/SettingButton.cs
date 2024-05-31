using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingButton : MonoBehaviour
{
    public FadeSceneLoader fadeSceneLoader;
    public TimeManager timeManager;
    [SerializeField] private GameObject[] Panels =null;


    public void MenuOnPush()
    {   

         Panels[2].SetActive(true);
        FadeScript fadeScript = GetComponent<FadeScript>();
        if (fadeScript != null)
        {
            fadeScript.StartFadeIn();
        }

        timeManager.OnStop();
    }

    public void MenuOffPush()
    {
        Panels[2].SetActive(false);

        timeManager.OnStart();
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
}
