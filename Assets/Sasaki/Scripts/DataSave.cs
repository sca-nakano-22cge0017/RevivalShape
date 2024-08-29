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
    // �ǉ�
    public string StageName; // �X�e�[�W��

    public bool IsClear; //�N���A���Ă��邩�ǂ���
    public int GotStar; //���̐�(�~�b�V�����N���A���)

    // �ǉ�
    public bool[] IsMissionClear = new bool[3]; // �e�~�b�V�����N���A��
}

public class DataSave : MonoBehaviour
{
    private static string dataPath = " "; // �t�@�C���p�X

    private static int stageAmount;
    /// <summary>
    /// �X�e�[�W���擾
    /// </summary>
    /// <returns>�`���[�g���A����G�N�X�g�����܂߂Ȃ��X�e�[�W��</returns>
    public static int GetStageAmount() { return stageAmount; }
    public static void SetStageAmount(int _stageAmount) { if (_stageAmount > 0) stageAmount = _stageAmount; }

    /// <summary>
    /// �t�@�C���̃p�X���擾����
    /// </summary>
    /// <returns>�t�@�C���̃p�X</returns>
    public static bool SetDataPath()
    {
        string _dataPath;

        //�p�X���w�肵�ēǂݍ���
        string directoryName = Application.persistentDataPath + "/Resources";

        //�f�B���N�g�����Ȃ�������
        while (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName); //����
        }
#if UNITY_EDITOR
        _dataPath = Application.dataPath + "/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        _dataPath = Application.persistentDataPath + "/Resources/AndroidJson.json";
#endif

        if (_dataPath == null || _dataPath == " ")
        {
            Debug.Log("�t�@�C���̃p�X���擾�ł��܂���ł����B");
        }

        dataPath = _dataPath;

        return true;
    }

    /// <summary>
    /// �t�@�C���̑��݊m�F
    /// </summary>
    /// <returns>�m�F����</returns>
    public static bool FileCheck()
    {
        //Json�t�@�C�����Ȃ���ΐ����A������
        while (!File.Exists(dataPath))
        {
            //�t�@�C������
            FileStream fs = File.Create(dataPath);
            fs.Close();
            Initialize();
        }

        return true; // �t�@�C���m�F����
    }

    /// <summary>
    /// �f�[�^������
    /// </summary>
    public static void Initialize()
    {
        PlayerData data = new PlayerData();

        // �X�e�[�W������������
        data.DataList = new();
        for (int i = 0; i <= stageAmount; i++)
        {
            StageData stageData = new StageData();

            if (i == 0) stageData.StageName = "Tutorial"; // ��ԍŏ��Ƀ`���[�g���A���̃f�[�^�����
            else stageData.StageName = "Stage" + i.ToString();

            stageData.IsClear = false;
            stageData.GotStar = 0;

            for (int m = 0; m < 3; m++)
            {
                stageData.IsMissionClear[m] = false;
            }

            data.DataList.Add(stageData);
        }

        var extraAmount = stageAmount / 10; // �G�N�X�g���X�e�[�W�̐�
        for (int i = 1; i <= extraAmount; i++)
        {
            StageData stageData = new StageData();

            stageData.StageName = "Extra" + i.ToString();

            stageData.IsClear = false;
            stageData.GotStar = 0;

            for (int m = 0; m < 3; m++)
            {
                stageData.IsMissionClear[m] = false;
            }

            data.DataList.Add(stageData);
        }

        // Tutorial -> Stage1�`100 -> Extra1�`20�̏��ŕۑ�

        SavePlayerData(data);
    }

    /// <summary>
    /// �Z�[�u
    /// </summary>
    /// <param name="_playerData">�f�[�^</param>
    public static void SavePlayerData(PlayerData _playerData)
    {
        StreamWriter writer = null;

        //player�f�[�^��Json�ɕϊ�
        string jsonstr = JsonUtility.ToJson(_playerData);

        //Json�t�@�C���ɏ�������
        writer = new StreamWriter(dataPath, false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// ���[�h
    /// </summary>
    /// <returns>�N���A�f�[�^</returns>
    public static PlayerData LoadPlayerData()
    {
        string datastr = "";
        StreamReader reader;
        reader = new StreamReader(dataPath);
        datastr = reader.ReadToEnd();
        reader.Close();

        //json�t�@�C�����I�u�W�F�N�g�ɕύX
        return JsonUtility.FromJson<PlayerData>(datastr);
    }

    /// <summary>
    /// �f�[�^���Z�b�g
    /// </summary>
    public static void DataReset()
    {
        if (File.Exists(dataPath))
        {
            File.Delete(dataPath);
        }

        FileCheck();
    }
}