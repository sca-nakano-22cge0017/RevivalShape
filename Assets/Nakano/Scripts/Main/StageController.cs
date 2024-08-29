using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Extensions;

/// <summary>
/// ���C���Q�[������
/// </summary>
public class StageController : MonoBehaviour
{
    // �t�F�[�Y
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = PHASE.CHECK;

    [SerializeField, Header("�X�e�[�W��")] private string stageName;
    public string StageName { get { return stageName; } }

    LoadManager loadManager;

    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageDataLoader stageDataLoader;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private MissionScore missionScore;
    [SerializeField] private Tutorial tutorial;

    [SerializeField] private CameraRotate cameraRotate;
    [SerializeField] private SheatCreate sheatCreate;

    // �e�t�F�[�Y�̏���
    [SerializeField] private CheckPhase checkPhase;
    [SerializeField] private SelectPhase selectPhase;
    [SerializeField] private PlayPhase playPhase;

    // UI����n
    [SerializeField] private TestButton testButton;
    [SerializeField] private GameObject optionButton;

    private bool isTutorial = false;
    /// <summary>
    /// �`���[�g���A���X�e�[�W���ǂ���
    /// </summary>
    public bool IsTutorial { get { return isTutorial;} private set { isTutorial = value; } }

    private bool canToCheckPhase = true; // �I���t�F�[�Y����m�F�t�F�[�Y�Ɉڍs�ł��邩
    [SerializeField, Header("�I�����m�F�ڍs���̊m�F�E�B���h�E")] private GameObject confirmWindow;
    [SerializeField] private Text numberOfReconfirmation;

    /// <summary>
    /// �T���v���̑傫��
    /// </summary>
    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // �f�[�^�擾�����������ǂ���
    private bool dataGot = false;

    /// <summary>
    /// �~�X��
    /// </summary>
    public int Miss { get; private set; } = 0;
    
    /// <summary>
    /// �Ċm�F��
    /// </summary>
    public int Reconfirmation { get; private set; } = 0;

    /// <summary>
    /// �g�p�}�`�̎�ވꗗ
    /// </summary>
    public ShapeData.Shape[] ShapeType { get; private set; }

    private ShapeData.Shape[,,] correctAnswer;
    /// <summary>
    /// ����������
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer
    {
        get
        {
            return correctAnswer;
        }
        private set
        {
            correctAnswer = value;
        }
    }

    private ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer
    {
        get
        {
            return playerAnswer;
        }
        set
        {
            playerAnswer = value;
        }
    }

    private bool isClear = false;
    /// <summary>
    /// true�ŃX�e�[�W�N���A
    /// </summary>
    public bool IsClear
    {
        get
        {
            return isClear;
        }
        set
        {
            isClear = value;
        }
    }

    private bool isRetry = false;
    /// <summary>
    /// true�̂Ƃ����g���C
    /// </summary>
    public bool IsRetry
    {
        get
        {
            return isRetry;
        }
        set
        {
            isRetry = value;
        }
    }

    private bool isPause = false;
    /// <summary>
    /// �|�[�Y��Ԃ��ǂ���
    /// </summary>
    public bool IsPause
    {
        get
        {
            return isPause;
        }
        set
        {
            isPause = value;
        }
    }

    private void Awake()
    {
        timeManager.OnStop();

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // �I���X�e�[�W�����擾

        dataGot = false;
        stageDataLoader.StageDataGet(stageName);  // �X�e�[�W�̔z�u�f�[�^�����[�h�J�n

        canToCheckPhase = true;
        confirmWindow.SetActive(false);

        // �`���[�g���A���Ȃ�
        if(stageName == "Tutorial")
        {
            isTutorial = true;
            tutorial.TutorialStart();
        }

        loadManager = FindObjectOfType<LoadManager>();
        if (loadManager != null)
        {
            // �t�F�[�h�I����X�^�[�g
            StartCoroutine(DelayCoroutine(() => { return loadManager.DidFadeComplete; },
                () => { timeManager.OnStart(); }));
        }
        else timeManager.OnStart();
    }

    void Update()
    {
        // ���[�h���I����Ă��Ȃ���Έȍ~�̏����ɐi�܂��Ȃ�
        if (!stageDataLoader.stageDataLoadComlete) return;

        if (phase != PHASE.PLAY && tutorial.TutorialCompleteByPhase) isPause = !timeManager.TimeActive;
        else isPause = false;

        // �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ���Ύ擾�E������
        if (!dataGot) Initialize();

        if ((loadManager != null && loadManager.DidFadeComplete) || loadManager == null)
        {
            MainGameManage();
            ClearOrRetry();
        }
    }

    /// <summary>
    /// �ϐ�/�e�t�F�[�Y�̏������@�m�F�t�F�[�Y�ւ̈ڍs
    /// </summary>
    private void Initialize()
    {
        // �}�b�v�T�C�Y�擾
        MapSize = stageDataLoader.LoadStageSize();

        // �z�� �v�f���w��
        ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
        CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
        PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
        playerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

        // �z��̏�����
        for (int z = 0; z < MapSize.z; z++)
        {
            for (int y = 0; y < MapSize.y; y++)
            {
                for (int x = 0; x < MapSize.x; x++)
                {
                    CorrectAnswer[x, y, z] = ShapeData.Shape.Empty;
                    PlayerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    playerAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
        for (int i = 0; i < ShapeType.Length; i++)
        {
            ShapeType[i] = ShapeData.Shape.Empty;
        }

        // �����擾
        CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

        // �g�p���Ă���}�`�̎�ނ��擾
        // ���̔z����ɂ���}�`�����A�I���t�F�[�Y�̃^�u���Ƀ{�^����\������
        ShapeType = shapeData.ShapeTypes(CorrectAnswer);

        // �e�N���X�̏�����
        cameraRotate.Initialize();
        checkPhase.Initialize();
        selectPhase.Initialize();
        playPhase.Initialize();

        // �V�[�g�쐬
        sheatCreate.Create();

        // �m�F�t�F�[�Y�Ɉڍs
        ToCheckPhase();

        dataGot = true;
    }

    /// <summary>
    /// �t�F�[�Y�Ǘ�
    /// </summary>
    private void MainGameManage()
    {
        if (IsTutorial) tutorial.TutorialUpdate();

        switch (phase)
        {
            case PHASE.CHECK:
                cameraRotate.CameraUpdate();
                break;
            
            case PHASE.SELECT:
                selectPhase.PhaseUpdate();
                break;

            case PHASE.PLAY:
                playPhase.PhaseUpdate();
                break;
        }
    }

    /// <summary>
    /// �N���A/���g���C���̏���
    /// </summary>
    private void ClearOrRetry()
    {
        if((isTutorial && !tutorial.IsTutorialComplete) || isPause) return;

        // �N���A���̑J�ڏ���
        if (IsClear && Input.touchCount >= 1)
        {
            // �X�e�[�W�I����ʂɖ߂�
            SceneLoader.Load("SelectScene");

            IsClear = false;
        }

        // �Ē��펞�̏���
        if (IsRetry && Input.touchCount >= 1)
        {
            // �m�F�t�F�[�Y�ɖ߂�
            testButton.BackToggle();
            IsRetry = false;
        }
    }

    /// <summary>
    /// �m�F�E�B���h�E�̕\��
    /// </summary>
    public void ConfirmDisp()
    {
        // �I���t�F�[�Y����m�F�t�F�[�Y�ɖ߂�{�^�����������Ƃ��ɌĂ΂��
        if (phase == PHASE.SELECT)
        {
            numberOfReconfirmation.text = "���݂̉񐔁F" + Reconfirmation.ToString() + "��";
            confirmWindow.SetActive(true);
            timeManager.OnStop();
        }
    }

    /// <summary>
    ///  �I���t�F�[�Y����m�F�t�F�[�Y�ֈڍs
    /// </summary>
    /// <param name="_canToCheckPhase">�m�F�t�F�[�Y�Ɉڍs���邩</param>
    public void Confirm(bool _canToCheckPhase)
    {
        // �m�F�E�B���h�E�̂͂�/�������̃{�^�����������Ƃ��ɌĂ΂��
        if (phase == PHASE.SELECT)
        {
            canToCheckPhase = _canToCheckPhase;
            confirmWindow.SetActive(false);

            // �m�F�t�F�[�Y�ɖ߂�Ƃ��i�w�͂��x�������ꂽ�Ƃ��j
            if (canToCheckPhase)
            {
                // �t�F�[�Y�ڍs
                selectPhase.PhaseEnd();
                ToCheckPhase();

                Reconfirmation++;

                testButton.BackToggle();
                timeManager.OnStart();
            }
        }
    }

    /// <summary>
    /// �m�F�t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToCheckPhase()
    {
        switch (phase)
        {
            case PHASE.SELECT:
                break;
            case PHASE.PLAY:
                playPhase.PhaseEnd();
                Miss++;
                cameraRotate.FromPlayPhase();
                break;
        }

        if (canToCheckPhase)
        {
            phase = PHASE.CHECK;

            // �ݒ�{�^��
            optionButton.SetActive(true);

            // �^�b�v��Ԃ̃��Z�b�g
            cameraRotate.TapReset();

            // �V�[�g�̕\���ݒ�
            sheatCreate.SheatDisp(true, true);

            // �m�F�t�F�[�Y�J�n����
            checkPhase.PhaseStart();

            canToCheckPhase = false;
        }
    }

    /// <summary>
    /// �I���t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToSelectPhase()
    {
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.PhaseEnd();
                break;
            case PHASE.PLAY:
                playPhase.PhaseEnd();
                break;
        }

        phase = PHASE.SELECT;

        if(isTutorial) tutorial.ToSelectA = true;

        canToCheckPhase = false;

        // �ݒ�{�^��
        optionButton.SetActive(true);

        // �V�[�g
        sheatCreate.SheatDisp(false, false);

        // �I���t�F�[�Y�J�n����
        selectPhase.PhaseStart();
    }

    /// <summary>
    /// ���s�t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToPlayPhase()
    {
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.PhaseEnd();
                break;
            case PHASE.SELECT:
                selectPhase.PhaseEnd();
                break;
        }

        phase = PHASE.PLAY;

        if (isTutorial) tutorial.ToPlayA();

        canToCheckPhase = true;

        // �ݒ�{�^����\��
        optionButton.SetActive(false);

        // �V�[�g
        sheatCreate.SheatDisp(true, false);

        // �J���� �ʒu�ύX
        cameraRotate.ToPlayPhase();

        // ���s�t�F�[�Y�J�n����
        playPhase.PhaseStart();
    }
}
