using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

/// <summary>
/// ���C���Q�[������
/// </summary>
public class StageController : MonoBehaviour
{
    // �t�F�[�Y
    public enum PHASE { CHECK = 0, SELECT, PLAY, };
    public PHASE phase = PHASE.CHECK;

    [SerializeField, Header("�X�e�[�W��")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;
    [SerializeField] private TapManager tapManager;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    [SerializeField] CheckPhase checkPhase;
    [SerializeField] SelectPhase selectPhase;
    [SerializeField] PlayPhase playPhase;

    [SerializeField] TestButton testButton;

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

    private ShapeData.Shape[,,] playerAnswer;
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

    [SerializeField, Header("�t���[�����[�g")] int fps = 120;

    [Header("�}�`�`��")]
    [SerializeField, Header("�`��͈� ����")] private Vector2 drawRangeMin = new Vector2(0, 370);
    [SerializeField, Header("�`��͈� �E��")] private Vector2 drawRangeMax = new Vector2(1080, 1700);
    [SerializeField] private Texture _texture;
    [SerializeField] private bool isDragRangeDraw = false;

    private void Awake()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        _texture = texture;

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage; // �I���X�e�[�W�����擾

        dataGot = false;
        stageDataLoader.StageDataGet(stageName);  // �X�e�[�W�̔z�u�f�[�^�����[�h�J�n

        Application.targetFrameRate = fps;
    }

    void Update()
    {
        // ���[�h���I����Ă��Ȃ���Ύ��̏����ɐi�܂��Ȃ�
        if (!stageDataLoader.stageDataLoadComlete) return;

        // �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ����
        if (!dataGot)
        {
            // �}�b�v�T�C�Y�擾
            MapSize = stageDataLoader.LoadStageSize();

            // �z�� �v�f���w��
            ShapeType = new ShapeData.Shape[System.Enum.GetValues(typeof(ShapeData.Shape)).Length];
            CorrectAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            PlayerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];
            playerAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

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

            cameraRotate.CameraSetting();

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

            dataGot = true;
        }

        // �N���A���̑J�ڏ���
        if (IsClear && Input.GetMouseButton(0))
        {
            if (!tapManager.TapOrDragRange(Input.mousePosition)) return;

            // �X�e�[�W�I����ʂɖ߂�
            if (!playPhase.IsDebug)
                SceneManager.LoadScene("SelectScene");
            IsClear = false;
        }

        // �Ē��펞�̏���
        if (IsRetry && Input.touchCount >= 1)
        {
            // �m�F�t�F�[�Y�ɖ߂�
            if (!playPhase.IsDebug)
            {
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
                selectPhase.SelectPhaseEnd();
                Reconfirmation++;
                break;
            case PHASE.PLAY:
                playPhase.PlayPhaseEnd();
                Miss++;
                cameraRotate.Restore();
                break;
        }

        phase = PHASE.CHECK;

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
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.CheckPhaseEnd();
                break;
            case PHASE.PLAY:
                playPhase.PlayPhaseEnd();
                break;
        }

        phase = PHASE.SELECT;

        // �J����
        cameraRotate.CanRotate = false;

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
        switch (phase)
        {
            case PHASE.CHECK:
                checkPhase.CheckPhaseEnd();
                break;
            case PHASE.SELECT:
                selectPhase.SelectPhaseEnd();
                break;
        }

        phase = PHASE.PLAY;

        // �V�[�g
        sheatCreate.SheatDisp(true, false);

        cameraRotate.PlayPhaseCamera();

        // ���s�t�F�[�Y�J�n����
        playPhase.PlayPhaseStart();
    }

    public IEnumerator DelayCoroutine(float seconds, Action action)
    {
        yield return new WaitForSeconds(seconds);
        action?.Invoke();
    }

    private void OnGUI()
    {
        if (isDragRangeDraw)
        {
            var rect = new Rect(drawRangeMin.x, drawRangeMin.y, drawRangeMax.x - drawRangeMin.x, drawRangeMax.y - drawRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
