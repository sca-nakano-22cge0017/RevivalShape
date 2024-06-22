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
    public GameObject Panel;
    public GameObject Panel1;
    public FadeScript fadeScript;
    public SettingButton settingButton;

   // [SerializeField]
    //private CanvasGroup canvasGroup;

    // Start is called before the first frame update

    public void Start()
    {
       

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FadeOut()
    {
        fadeScript.StartFadeOut();
        StartCoroutine(FadeOutOk());
      
        
        soundManager.SEPlay6();
        Debug.Log("se");
     
    }
    public void FadeIn()
    {
        fadeScript.StartFadeIn();
       
        StartCoroutine(FadeOutOff());
       
       

        //  Panel.SetActive(false);
    }
    IEnumerator FadeOutOk()
    {
        timeManager.OnStop();
       // Panel1.SetActive(true);
      
        yield return new WaitForSeconds(0.3f);
      Panel.SetActive(true);
        yield return new WaitForSeconds(0f);
     
      Panel1.SetActive(true);
        //animationOnFide.PlayAnim();
        


        Debug.Log("è≠Çµë“Ç¬");
    }
    IEnumerator FadeOutOff()
    {

        yield return new WaitForSeconds(0.2f);
       Panel.SetActive(false);
      Panel1.SetActive(false);
    
        timeManager.OnStart();
        Debug.Log("è≠Çµë“Ç¬");
    }

  
}

