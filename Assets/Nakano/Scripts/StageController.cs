using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = 0;

    [SerializeField, Header("�X�e�[�W��")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    [SerializeField] CheckPhase checkPhase;
    [SerializeField] SelectPhase selectPhase;
    [SerializeField] PlayPhase playPhase;

    private int yDataMax = 10; //�I���t�F�[�Y��1�}�X���ɓ��͂ł���ő�l

    Vector3 mapSize = new Vector3(4, 4, 4);
    public Vector3 MapSize { get { return mapSize; } }

    int playerAnswerSizeY = 0;
    public int PlayerAnswerSizeY { get { return playerAnswerSizeY; } }

    private int phaseBackCount = 0;
    /// <summary>
    /// �m�F�t�F�[�Y�ɖ߂�����
    /// </summary>
    public int PhaseBackCount { get { return phaseBackCount; } set { phaseBackCount = value; } }

    ShapeData.Shape[] shapeType; // �g�p�}�`�̎��
    private int shapeTypeAmount = 0; // �g�p�}�`�̎�ސ�

    ShapeData.Shape[,,] correctAnswer;
    /// <summary>
    /// ����������
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer
    {
        get { return correctAnswer; }
        set { correctAnswer = value; }
    }

    ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer
    {
        get { return playerAnswer; }
        set { playerAnswer = value; }
    }

    // �f�[�^�擾�����������ǂ���
    bool mapSizeDataGot = false;
    bool stageDataGot = false;

    private void Awake()
    {
        stageDataLoader.StageDataGet(stageName);
    }

    void Update()
    {
        // �f�[�^�̃��[�h���������Ă����� ���� �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ����
        if(stageDataLoader.stageDataLoadComlete && !mapSizeDataGot)
        {
            // �}�b�v�T�C�Y�擾
            mapSize = stageDataLoader.LoadStageSize(stageName);
            shapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;
            mapSize.y = yDataMax * shapeTypeAmount; // �}�`�̎�ސ��ɉ����ăv���C���[�̉𓚗p�̔z��̃T�C�Y��ύX����

            // �z�� �v�f���w��
            shapeType = new ShapeData.Shape[shapeTypeAmount];
            correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
            playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

            // �z�u�f�[�^���[�h
            stageDataLoader.StageDataGet(stageName);
            cameraRotate.MapSizeInitialize();

            mapSizeDataGot = true;
        }

        // �����̔z�u�f�[�^��z��ɓ����
        if (stageDataLoader.stageDataLoadComlete && !stageDataGot)
        {
            correctAnswer = stageDataLoader.LoadStageMap(mapSize);

            shapeType = shapeData.ShapeTypes(correctAnswer); // �g�p���Ă���}�`�̎�ނ��擾

            sheatCreate.Sheat(); // �V�[�g�쐬
            ToCheckPhase(); // �m�F�t�F�[�Y�Ɉڍs

            stageDataGot = true;
        }
    }

    /// <summary>
    /// �m�F�t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToCheckPhase()
    {
        phase = PHASE.CHECK;

        playPhase.PlayPhaseEnd();
        selectPhase.SelectPhaseEnd();

        cameraRotate.CanRotate = true; // �J�����̉�]���ł���悤�ɂ���

        sheatCreate.SheatDisp(true, true); // �V�[�g�̕\���ݒ�

        checkPhase.CheckPhaseStart(); // �m�F�t�F�[�Y�Ɉڍs����
    }

    /// <summary>
    /// �I���t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToSelectPhase()
    {
        phase = PHASE.SELECT;

        checkPhase.CheckPhaseEnd();
        playPhase.PlayPhaseEnd();

        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

        sheatCreate.SheatDisp(false, false);

        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// ���s�t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToPlayPhase()
    {
        phase = PHASE.PLAY;

        checkPhase.CheckPhaseEnd();
        selectPhase.SelectPhaseEnd();

        sheatCreate.SheatDisp(true, false);
        
        playPhase.PlayPhaseStart();
    }
}
