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

    bool mapSizeDataGot = false;
    bool stageDataGot = false;

    void Update()
    {
        // �f�[�^�̃��[�h���������Ă����� ���� �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ����
        if(stageDataLoader.mapSizeDataLoadComlete && !mapSizeDataGot)
        {
            // �}�b�v�T�C�Y�擾
            mapSize = stageDataLoader.LoadStageSize(stageName);
            mapSize.y = yDataMax;

            // �z�� �v�f���w��
            correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
            playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

            mapSizeDataGot = true;

            // �z�u�f�[�^���[�h
            stageDataLoader.StageDataGet(stageName);

            cameraRotate.MapSizeInitialize();
        }

        // �����̔z�u�f�[�^��z��ɓ����
        if (stageDataLoader.stageDataLoadComlete && !stageDataGot)
        {
            correctAnswer = stageDataLoader.LoadStageMap(mapSize);

            stageDataGot = true;

            sheatCreate.Sheat(); // �V�[�g�쐬
            ToCheckPhase();
        }
    }

    /// <summary>
    /// �m�F�t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToCheckPhase()
    {
        phase = PHASE.CHECK;
        checkPhase.CheckPhaseStart(); // �m�F�t�F�[�Y�Ɉڍs����

        cameraRotate.CanRotate = true; // �J�����̉�]���ł���悤�ɂ���
    }

    /// <summary>
    /// �I���t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToSelectPhase()
    {
        phase = PHASE.SELECT;
        checkPhase.CheckPhaseEnd();

        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// ���s�t�F�[�Y�Ɉڍs
    /// </summary>
    public void ToPlayPhase()
    {
        phase = PHASE.PLAY;
        selectPhase.SelectPhaseEnd();

        sheatCreate.PlayPhase();
        StartCoroutine(PlayPhaseStart());
    }

    IEnumerator PlayPhaseStart()
    {
        yield return new WaitForSeconds(1f);

        playPhase.PlayPhaseStart();
    }
}
