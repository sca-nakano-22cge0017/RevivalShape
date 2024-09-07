using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static PlayerData playerData = null;

    [SerializeField, Header("�`���[�g���A����G�N�X�g�����܂߂Ȃ��X�e�[�W��")] private int stageAmount = 50;
    [SerializeField, Header("�t���[�����[�g")] int fps = 120;

    private const int missionAmount = 3; // �~�b�V������

    private static bool didLoad = false; // �f�[�^���[�h������
    public static bool DidLoad
    {
        get { return didLoad; }
        private set { }
    }

    // �V���O���g��
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        FpsSet();
        DataSave.SetStageAmount(stageAmount);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        // �t�@�C�������݂��邩�̊m�F������������
        if (DataSave.SetDataPath() && DataSave.FileCheck() && !didLoad)
        {
            while (DataSave.LoadPlayerData() == null)
            {
                // �f�[�^��������Ώ�����
                DataSave.Initialize();
            }

            playerData = DataSave.LoadPlayerData();
            didLoad = true; // �ǂݍ��݊���
        }
    }

    void FpsSet()
    {
        Application.targetFrameRate = fps;
    }

    /// <summary>
    /// �f�[�^���Z�b�g
    /// </summary>
    public static void DataReset()
    {
        DataSave.DataReset();

        // �ēǂݍ���
        didLoad = false;
    }

    /// <summary>
    /// �X�e�[�W�S�J��
    /// </summary>
    public static void AllRelease()
    {
        bool[] mission = { true, true, true };
        StageDataUpdate("Tutorial", true, mission);

        for (int i = 1; i <= DataSave.GetStageAmount(); i++)
        {
            StageDataUpdate("Stage" + i.ToString(), true, mission);
        }
    }

    /// <summary>
    /// �X�e�[�W�̃N���A�󋵓����X�V�E�ۑ�
    /// </summary>
    /// <param name="stageName"></param>
    public static void StageDataUpdate(string _stageName, bool _isClear, bool[] _isMissionClear)
    {
        // ����ۑ������X�e�[�W�̖��O�ƁA�X�e�[�W������v����DataList�̗v�f���擾����
        foreach (var d in playerData.DataList)
        {
            if (_stageName.ToLower() == d.StageName.ToLower())
            {
                // �f�[�^����
                d.IsClear = _isClear;

                for (int i = 0; i < missionAmount; i++)
                {
                    if (!d.IsMissionClear[i])
                    {
                        d.IsMissionClear[i] = _isMissionClear[i];
                    }
                    
                    if (_isMissionClear[i] == true && d.GotStar < 3)
                    {
                        d.GotStar++;
                    }
                }
            }
        }

        // �Z�[�u
        DataSave.SavePlayerData(playerData);
    }

    /// <summary>
    /// �w�肵���X�e�[�W�̃f�[�^���擾����
    /// </summary>
    /// <param name="_stageName">�X�e�[�W��</param>
    /// <returns>�X�e�[�W�f�[�^</returns>
    public static StageData GetStageData(string _stageName)
    {
        StageData data = new();

        foreach (var d in playerData.DataList)
        {
            if (_stageName.ToLower() == d.StageName.ToLower())
            {
                data = d;
            }
        }

        return data;
    }
}