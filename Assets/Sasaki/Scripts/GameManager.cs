using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PlayerData playerData;
    DataSave dataSave = new DataSave();

    void Awake()
    {
        DontDestroyOnLoad(this);
        if (dataSave.LoadPlayerData() == null)
        {
            playerData.DataList = new();
        }
        else
        {
            playerData = dataSave.LoadPlayerData();
        }
    }
}
