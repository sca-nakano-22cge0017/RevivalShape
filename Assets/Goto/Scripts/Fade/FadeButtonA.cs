using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FadeButtonA : MonoBehaviour
{
    public TimeManager timeManager;
    public GameObject Panel;
    public FadeScript fadeScript;
    public SettingButton settingButton;
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
        yield return new WaitForSeconds(0.5f);
        Panel.SetActive(true);
       
        Debug.Log("è≠Çµë“Ç¬");
    }
    IEnumerator FadeOutOff()
    {

        yield return new WaitForSeconds(0.2f);
         Panel.SetActive(false);
       
        timeManager.OnStart();
        Debug.Log("è≠Çµë“Ç¬");
    }
}
