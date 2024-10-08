using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class FadeButtonA : MonoBehaviour
{
    //public FadeController controller;
    public SoundManager soundManager;
    public AnimationOnFide animationOnFide;
    public TimeManager timeManager;
    public GameObject FadePanel;
    public GameObject MainPanel;
    public FadeScript fadeScript;
    public SettingButton settingButton;

   // [SerializeField]
    //private CanvasGroup canvasGroup;

    // Start is called before the first frame update

    public void Start()
    {
        soundManager = FindObjectOfType<SoundManager>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeOut()
    {
        //fadeScript.StartFadeOut();
        //StartCoroutine(FadeOutOk());

        timeManager.OnStop();
        FadePanel.SetActive(true);

        if(soundManager != null) soundManager.SEPlay6();
        Debug.Log("se");
     
    }
    public void FadeIn()
    {
        //fadeScript.StartFadeIn();
        //StartCoroutine(FadeOutOff());

        FadePanel.SetActive(false);
        timeManager.OnStart();

        //  Panel.SetActive(false);
    }
    IEnumerator FadeOutOk()
    {
        timeManager.OnStop();
       // Panel1.SetActive(true);
      
        yield return new WaitForSeconds(0.3f);
      FadePanel.SetActive(true);
        yield return new WaitForSeconds(0f);
      MainPanel.SetActive(true);
        //animationOnFide.PlayAnim();
        Debug.Log("少し待つ");
    }
    IEnumerator FadeOutOff()
    {
        yield return new WaitForSeconds(0.2f);
       FadePanel.SetActive(false);
       MainPanel.SetActive(false);
    
        timeManager.OnStart();
        Debug.Log("少し待つ");
    }

  
}

