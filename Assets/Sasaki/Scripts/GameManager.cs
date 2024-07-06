using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerData playerData = null;
    [SerializeField] private DataSave dataSave;

    // �ǉ�
    [SerializeField, Header("�`���[�g���A����G�N�X�g�����܂߂Ȃ��X�e�[�W��")] private int stageAmount = 20;
    public int StageAmount { get { return stageAmount; } private set { } }
    private int missionAmount = 3; // �~�b�V������
    private bool didLoad = false; // �f�[�^���[�h������
    public bool DidLoad { get { return didLoad;} set { } }

    [SerializeField, Header("�t���[�����[�g")] int fps = 120;

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
    }

    private void Start()
    {
        Application.targetFrameRate = fps;
        DontDestroyOnLoad(this);

        if (dataSave != null)
        {
            dataSave.FileCheck();
        }
    }

    private void Update()
    {
        // �t�@�C�������݂��邩�̊m�F������������
        if (dataSave.DidFileChecked && !didLoad)
        {
            while (dataSave.LoadPlayerData() == null)
            {
                //playerData.DataList = new();

                // �f�[�^��������Ώ�����
                dataSave.Initialize();
            }

            while (playerData == null)
            {
                playerData = dataSave.LoadPlayerData();
            }

            didLoad = true; // �ǂݍ��݊���
        }

        if(Input.GetKeyDown(KeyCode.Delete))
        {
            dataSave.Initialize();
        }
    }

    /// <summary>
    /// �X�e�[�W�̃N���A�󋵓����X�V�E�ۑ�
    /// </summary>
    /// <param name="stageName"></param>
    public void StageDataUpdate(string _stageName, bool _isClear, bool[] _isMissionClear)
    {
        // ����ۑ������X�e�[�W�̖��O�ƁA�X�e�[�W������v����DataList�̗v�f���擾����
        foreach (var d in playerData.DataList)
        {
            if(_stageName.ToLower() == d.StageName.ToLower())
            {
                // �f�[�^����
                d.IsClear = _isClear;

                for (int i = 0; i < missionAmount; i++)
                {
                    d.IsMissionClear[i] = _isMissionClear[i];

                    if(_isMissionClear[i] == true && d.GotStar < 3)
                    {
                        d.GotStar++;
                    }
                }
            }
        }

        // �Z�[�u
        dataSave.SavePlayerData(playerData);
    }

    /// <summary>
    /// �w�肵���X�e�[�W�̃f�[�^���擾����
    /// </summary>
    /// <param name="_stageName">�X�e�[�W��</param>
    /// <returns>�X�e�[�W�f�[�^</returns>
    public StageData GetStageData(string _stageName)
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
