using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondSelectButton : MonoBehaviour
{
    [HideInInspector] public SelectButton selectButton;
     public string stageName;

    public void OnClick()
    {
        if(selectButton) selectButton.ButtonName(stageName);
    }
}
