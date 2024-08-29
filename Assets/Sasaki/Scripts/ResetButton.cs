using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetButton : MonoBehaviour
{
    DataSave dataSave;
    private void Awake()
    {
        dataSave = GameObject.FindObjectOfType<DataSave>();
    }

    public void Reset()
    {
        //dataSave.RestartButton();
    }
}
