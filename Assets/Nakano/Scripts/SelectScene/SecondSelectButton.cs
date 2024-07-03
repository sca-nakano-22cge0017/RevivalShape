using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecondSelectButton : MonoBehaviour
{
    [HideInInspector] public SelectButton selectButton;
    [HideInInspector] public string stageName;

    public void OnClick()
    {
        selectButton.ButtonName(stageName);
    }
}
