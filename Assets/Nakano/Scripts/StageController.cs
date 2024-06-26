using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

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

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // �f�[�^�擾�����������ǂ���
    bool dataGot = false;

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

    [SerializeField, Header("�X���C�v�͈̔� �ŏ�")] Vector2 dragRangeMin;
    [SerializeField, Header("�X���C�v�͈̔� �ő�")] Vector2 dragRangeMax;

    [SerializeField, Header("�t���[�����[�g")] int fps = 120;

    // �^�b�v/�X���C�v�\�͈͂�`��
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

    private void Awake()
    {
        if(SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // �I���X�e�[�W�����擾

        stageDataLoader.StageDataGet(stageName);  // �X�e�[�W�̔z�u�f�[�^�����[�h�J�n

        Application.targetFrameRate = fps;

        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        _texture = texture;
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
            
            // �z�� �v�f���w��
            ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            for (int z = 0; z < MapSize.z; z++)
            {
                for (int y = 0; y < MapSize.y; y++)
                {
                    for (int x = 0; x < MapSize.x; x++)
                    {
                        CorrectAnswer[x, y, z] = ShapeData.Shape.Empty;
                        PlayerAnswer[x, y, z] = ShapeData.Shape.Empty;
                    }
                }
            }

            cameraRotate.TargetSet();

            // �����擾
            CorrectAnswer = stageDataLoader.LoadStageMap(MapSize);

            // �g�p���Ă���}�`�̎�ނ��擾
            ShapeType = shapeData.ShapeTypes(CorrectAnswer);

            checkPhase.Initialize();
            selectPhase.Initialize();
            playPhase.Initialize();

            // �V�[�g�쐬
            sheatCreate.Sheat();

            // �m�F�t�F�[�Y�Ɉڍs
            ToCheckPhase();
        }

        // �N���A���̑J�ڏ���
        if (IsClear && Input.GetMouseButton(0))
        {
            if (!TapOrDragRange(Input.mousePosition)) return;

            // �X�e�[�W�I����ʂɖ߂�
            if (!playPhase.IsDebug)
                SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // �Ē��펞�̏���
        if (IsRetry && Input.GetMouseButton(0))
        {
            if(!TapOrDragRange(Input.mousePosition)) return;

            // �m�F�t�F�[�Y�ɖ߂�
            if (!playPhase.IsDebug)
            {
                ToCheckPhase();
                TestButton testButton = GameObject.FindObjectOfType<TestButton>();
                testButton.BackToggle();
            }
                
            IsRetry = false;
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
                Reconfirmation++;
                break;
            case PHASE.PLAY:
                Miss++;
                cameraRotate.Restore();
                break;
        }

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

        cameraRotate.PlayPhaseCamera();

        // ���s�t�F�[�Y�J�n����
        playPhase.PlayPhaseStart();
    }

    /// <summary>
    /// �^�b�v/�h���b�O�ł��邩�𔻒肷��@�͈͊O���^�b�v���Ă���Ȃ�false
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos)
    {
        bool canTap = false;

        var p = _pos;
        if (p.x <= dragRangeMin.x || p.x > dragRangeMax.x || p.y <= dragRangeMin.y || p.y > dragRangeMax.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }

    /// <summary>
    /// �^�b�v/�h���b�O�ł��邩�𔻒肷��@�͈͊O���^�b�v���Ă���Ȃ�false
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <param name="_minPos">�͈́@�ŏ��l</param>
    /// <param name="_maxPos">�͈́@�ő�l</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos, Vector3 _minPos, Vector3 _maxPos)
    {
        bool canTap = false;

        if (_pos.x <= _minPos.x || _pos.x > _maxPos.x || _pos.y <= _minPos.y || _pos.y > _maxPos.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }

    /// <summary>
    /// �^�b�v/�h���b�O�ł���͈͂��擾
    /// </summary>
    /// <returns>�͈͂̍ŏ�/�ő�̍��W���Ԃ��Ă���</returns>
    public Vector2[] GetTapOrDragRange()
    {
        Vector2[] r = {dragRangeMin, dragRangeMax};
        return r;
    }

    private void OnGUI()
    {
        if(isDragRangeDraw)
        {
            var rect = new Rect(dragRangeMin.x, dragRangeMin.y, dragRangeMax.x - dragRangeMin.x, dragRangeMax.y - dragRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
