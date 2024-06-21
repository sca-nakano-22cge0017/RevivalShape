using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FadeButtonA : MonoBehaviour
{
    public AnimationOnFide animationOnFide;
    public TimeManager timeManager;
    public GameObject Panel;
    public GameObject Panel1;
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
        yield return new WaitForSeconds(0.3f);
        Panel1.SetActive(true);
        animationOnFide.PlayAnim();
        Debug.Log("アニメーション");


        Debug.Log("少し待つ");
    }
    IEnumerator FadeOutOff()
    {

        yield return new WaitForSeconds(0.2f);
         Panel.SetActive(false);
        Panel1.SetActive(false);

        timeManager.OnStart();
        Debug.Log("少し待つ");
    }
}
