using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

//Player�̃N���A��Ԃ�ۑ�����
[System.Serializable]
public class PlayerData
{
    public List<StageData> DataList = new();
}

[System.Serializable]
public class StageData
{
    public bool IsClear; //�N���A���Ă��邩�ǂ���
    public int GotStar; //���̐�(�~�b�V�����N���A���)
}

public class DataSave : MonoBehaviour
{
    string datapath;

    PlayerData playerData;

    private void Awake()
    {
        //�p�X���w�肵�ēǂݍ���
        string directoryName = Application.persistentDataPath + "/Resources";

        //�f�B���N�g�����Ȃ�������
        while (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName); //����
        }
#if UNITY_EDITOR
        datapath = Application.dataPath + "/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        datapath = Application.persistentDataPath + "/Resources/AndroidJson.json";
#endif
        //Json�t�@�C�����Ȃ���ΐ����A������
        while (!File.Exists(datapath))
        {
            //�t�@�C������
            FileStream fs = File.Create(datapath);
            fs.Close();
            Initialize();
        }

        //Json�t�@�C��������΃��[�h
        if (File.Exists(datapath))
        {
            playerData = LoadPlayerData();
        }
        else
        {
            Debug.Log("�t�@�C�������݂��܂���");
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
    public void Initialize()
    {
        PlayerData data = new PlayerData();
        StageData stageData = new StageData();

        stageData.IsClear = false;
        stageData.GotStar = 0;

        //���X�g�̒��Ƀf�[�^����
        data.DataList.Add(stageData);

        SavePlayerData(data);
    }
}
