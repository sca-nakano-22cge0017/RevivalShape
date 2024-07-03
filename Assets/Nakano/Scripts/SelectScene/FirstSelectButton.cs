using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FirstSelectButton : MonoBehaviour
{
    [HideInInspector] public SelectButtonController sbController;
    [HideInInspector] public int num;

    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        sbController.FirstSelect(num);
    }
}
