using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// ���C���Q�[������
/// </summary>
public class StageController : MonoBehaviour
{
    // �t�F�[�Y
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

    //�I���t�F�[�Y��1�}�X���ɓ��͂ł���ő�l
    private int yDataMax = 10;

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // �f�[�^�擾�����������ǂ���
    bool dataGot = false;

    /// <summary>
    /// �m�F�t�F�[�Y�ɖ߂�����
    /// </summary>
    public int PhaseBackCount { get; set; } = 0;

    /// <summary>
    /// �g�p�}�`�̎�ވꗗ
    /// </summary>
    public ShapeData.Shape[] ShapeType { get; private set; }

    /// <summary>
    /// �g�p�}�`�̎�ސ�
    /// </summary>
    public int ShapeTypeAmount { get; private set; } = 0;

    /// <summary>
    /// ����������
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer { get; set; }

    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer { get; set; }

    /// <summary>
    /// true�ŃX�e�[�W�N���A
    /// </summary>
    public bool IsClear { get; set; } = false;

    /// <summary>
    /// true�̂Ƃ����g���C
    /// </summary>
    public bool IsRetry { get; set; } = false;

    private void Awake()
    {
        if(SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // �I���X�e�[�W�����擾

        stageDataLoader.StageDataGet(stageName);  // �X�e�[�W�̔z�u�f�[�^�����[�h�J�n
    }

    void Update()
    {
        // ���[�h���I����Ă��Ȃ���Ύ��̏����ɐi�܂��Ȃ�
        if (!stageDataLoader.stageDataLoadComlete) return;

        // �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ����
        if(!dataGot)
        {
            dataGot = true;

            // �}�b�v�T�C�Y�擾
            MapSize = stageDataLoader.LoadStageSize();
            
            // �Q�[���ɓo�ꂷ��}�`�̎�ސ����擾
            ShapeTypeAmount = System.Enum.GetValues(typeof(ShapeData.Shape)).Length;

            // �}�`�̎�ސ��ɉ����ăv���C���[�̉𓚗p�̔z��̃T�C�Y��ύX����
            MapSize = new Vector3(MapSize.x, yDataMax * ShapeTypeAmount, MapSize.z);

            // �z�� �v�f���w��
            ShapeType = new ShapeData.Shape[ShapeTypeAmount];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            cameraRotate.TargetSet();

            // �����擾
            CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

            // �g�p���Ă���}�`�̎�ނ��擾
            ShapeType = shapeData.ShapeTypes(CorrectAnswer);

            // �g�p���Ă���}�`�̎�ސ����擾
            ShapeTypeAmount = shapeData.ShapeTypesAmount(ShapeType);

            // �V�[�g�쐬
            sheatCreate.Sheat();

            // �m�F�t�F�[�Y�Ɉڍs
            ToCheckPhase();
        }

        // �N���A���̑J�ڏ���
        if (IsClear && Input.GetMouseButton(0))
        {
            // �X�e�[�W�I����ʂɖ߂�
            SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // �Ē��펞�̏���
        if (IsRetry && Input.GetMouseButton(0))
        {
            // �m�F�t�F�[�Y�ɖ߂�
            ToCheckPhase();
            IsRetry = false;
        }
    }

    /// <summary>
    /// �m�F�t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToCheckPhase()
    {
        phase = PHASE.CHECK;

        playPhase.PlayPhaseEnd();
        selectPhase.SelectPhaseEnd();

        // �J�����̉�]���ł���悤�ɂ���
        cameraRotate.CanRotate = true;

        // �V�[�g�̕\���ݒ�
        sheatCreate.SheatDisp(true, true);

        // �m�F�t�F�[�Y�J�n����
        checkPhase.CheckPhaseStart();
    }

    /// <summary>
    /// �I���t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToSelectPhase()
    {
        phase = PHASE.SELECT;

        checkPhase.CheckPhaseEnd();
        playPhase.PlayPhaseEnd();

        // �J����
        cameraRotate.CanRotate = false;
        cameraRotate.RotateReset();

        // �V�[�g
        sheatCreate.SheatDisp(false, false);

        // �I���t�F�[�Y�J�n����
        selectPhase.SelectPhaseStart();
    }

    /// <summary>
    /// ���s�t�F�[�Y�Ɉڍs
    /// �t�F�[�Y���Ǘ�����enum�^�ϐ��̕ύX�A���t�F�[�Y�p��UI��\����
    /// </summary>
    public void ToPlayPhase()
    {
        phase = PHASE.PLAY;

        checkPhase.CheckPhaseEnd();
        selectPhase.SelectPhaseEnd();

        // �V�[�g
        sheatCreate.SheatDisp(true, false);

        // ���s�t�F�[�Y�J�n����
        playPhase.PlayPhaseStart();
    }
}
