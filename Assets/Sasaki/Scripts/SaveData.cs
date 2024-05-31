using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.AddressableAssets.Build;

[System.Serializable]
public class PlayerData
{
    public bool clearCheck; //�N���A���Ă��邩�ǂ���
    public int star;
}

public class SaveData : MonoBehaviour
{
    string datapath;

    private void Awake()
    {
#if UNITY_EDITOR
        datapath = Application.dataPath + "Assets/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        datapath = Application.persistentDataPath + "Assets/Resources/AndJson.json";
#endif
    }

    private void Start()
    {
        //Json�t�@�C��������΃��[�h�A�Ȃ���Ώ�����
        PlayerData player = new PlayerData();
        if (File.Exists(datapath))
        {
            player = LoadPlayerData();
        }
        else
        {
            Initialize(player);
        }
    }

    //�Z�[�u
    public void SavePlayerData(PlayerData player)
    {
        StreamWriter writer = null;

        //player�f�[�^��Json�ɕϊ�
        string jsonstr = JsonUtility.ToJson(player);

        //Json�t�@�C���ɏ�������
        writer = new StreamWriter(datapath, false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }

    //Json�t�@�C����ǂݍ��݁A���[�h����
    public PlayerData LoadPlayerData()
    {
        string datastr = "";
        StreamReader reader;
        reader = new StreamReader(datapath);
        datastr = reader.ReadToEnd();
        reader.Close();

        //json�t�@�C�����I�u�W�F�N�g�ɕύX
        return JsonUtility.FromJson<PlayerData> (datastr);
    }

    //Json�t�@�C�����Ȃ��ꍇ�����l���Z�[�u����������
    public void Initialize(PlayerData player)
    {
        player.clearCheck = false;
        player.star = 0;

        SavePlayerData(player);
    }
}
