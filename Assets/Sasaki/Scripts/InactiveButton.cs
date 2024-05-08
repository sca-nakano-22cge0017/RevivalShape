using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InactiveButton : MonoBehaviour
{
    [SerializeField, Header("Žg‚¦‚È‚¢ƒ{ƒ^ƒ“")] GameObject[] button;

    void Start()
    {
        for (int i = 0; i < button.Length; i++)
        {
            button[i].GetComponent<Button>().interactable = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
