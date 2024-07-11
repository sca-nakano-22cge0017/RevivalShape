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
    string datapath;

    PlayerData playerData;

    // �ǉ�
    [SerializeField] private GameManager gameManager;
    private int stageAmount = 0; // �`���[�g���A����G�N�X�g�����܂߂Ȃ��X�e�[�W��
    private bool didFileChecked = false; // �t�@�C���m�F�ς݂�
    public bool DidFileChecked { get { return didFileChecked; } private set { didFileChecked = value; } }

    private void Awake()
    {
        stageAmount = gameManager.StageAmount; // �X�e�[�W���擾

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
        //StageData stageData = new StageData();

        //stageData.IsClear = false;
        //stageData.GotStar = 0;

        ////���X�g�̒��Ƀf�[�^����
        //data.DataList.Add(stageData);

        // �ǉ� �X�e�[�W������������
        data.DataList = new();
        for (int i = 0; i < stageAmount; i++)
        {
            StageData stageData = new StageData();

            if(i == 0) stageData.StageName = "Tutorial"; // ��ԍŏ��Ƀ`���[�g���A���̃f�[�^�����
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

    //�t�@�C�����Z�b�g
    public void RestartButton()
    {
        Debug.Log("test" + File.Exists(datapath));
        if (File.Exists(datapath))
        {
            File.Delete(datapath);
        }

        FileCheck();

        gameManager.DataReset(); // GameManager���̃f�[�^����������
    }


    /// <summary>
    /// �t�@�C���̑��݊m�F
    /// </summary>
    public void FileCheck()
    {
        //Json�t�@�C�����Ȃ���ΐ����A������
        // Todo Json�t�@�C���̌`�����������Ȃ���΃t�@�C���𐶐�������
        while (!File.Exists(datapath))
        {
            //�t�@�C������
            FileStream fs = File.Create(datapath);
            fs.Close();
            Initialize();
        }

        didFileChecked = true; // �t�@�C���m�F����
    }
}
