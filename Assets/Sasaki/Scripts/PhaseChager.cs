using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseChager : MonoBehaviour
{
    [SerializeField] private Toggle[] toggles = null;
    private StageController sc;

    void Start()
    {
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
            Debug.Log("select");
        }
    }
    public void Oncheck()
    {
        if (toggles[1].isOn)
        {
            //sc.ToCheckPhase();
            Debug.Log("check");
        }
    }
    public void OnPlay()
    {
        //100%–¢–ž‚¾‚Á‚½‚çselect‚É–ß‚·
        if (toggles[2].isOn)
        {
            //sc.ToPlayPhase();
            Debug.Log("play");
        }
    }
}
