using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static Extensions;

[System.Serializable]
public class Combiners
{
    public ShapeData.Shape shape;
    [HideInInspector] public bool isShapeUsed = false;
    public MeshCombiner meshCombiner;
    public Transform parent;
}

/// <summary>
/// ���s�t�F�[�Y
/// </summary>
public class PlayPhase : MonoBehaviour, IPhase
{
    [SerializeField] private StageController stageController;
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private CameraRotate cameraRotate;
    [SerializeField] private Vibration vibration;
    [SerializeField] private SheatCreate sheatCreate;
    [SerializeField] private Combiners[] combiners;

    [SerializeField] private Transform objParent; // �e�I�u�W�F�N�g

    [SerializeField] private GameObject playPhaseUI;

    private string stageName;
    private Vector3 mapSize = new Vector3(0, 0, 0);

    private ShapeData.Shape[,,] map; // �z�u�f�[�^
    private GameObject[,,] mapObj;   // GameObject�^�z��

    private ShapeData.Shape[,,] correctAnswer; // ����
    private ShapeData.Shape[,,] lastAnswer;    // �O�̉�

    private bool isFalling = false;

    // �����X�L�b�v
    private bool isSkip = false;

    // ������
    public bool IsFastForward { get; private set; } = false;
    [field: SerializeField, Header("������̔{��")] public float FastForwardRatio { get; private set; }

    [SerializeField, Header("�������̐U���̒���(�b) �ʏ�")] private float fallVibrateTime_Normal;
    [SerializeField, Header("�������̐U���̒���(�b) ������")] private float fallVibrateTime_FastForward;
    [SerializeField, Header("�N���A���̐U���̒���(�b)")] private float clearVibrateTime;

    [SerializeField, Header("�������x")] private float fallSpeed;
    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ�����")] private int fallPos;
    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ��Ԋu(sec)")] private float fallInterval;
    [SerializeField, Tooltip("�I�u�W�F�N�g���S�ė������Ă����v���\���܂ł̎���(sec)")]
    private float fallToMatchdispTime;

    // ��v���v�Z
    private float matchRateTroutTotal = 0; // 1�}�X������̈�v���̘a
    private int hasBlockTrout = 0; // 1�ȏ�u���b�N������}�X�̐�
    private int matchRate = 0; // ��v��

    [SerializeField] private Text matchRateText;
    [SerializeField] private GameObject matchUI;
    [SerializeField] private GameObject completeText;
    [SerializeField] private Animator completeAnim;
    [SerializeField] private ParticleSystem completeEffect;

    private bool toClearWindow = false;
    [SerializeField] private ResultWindow resultWindow;
    [SerializeField] private MissionWindow missionWindow;
    [SerializeField] private MissionScore missionScore;

    /// <summary>
    /// �N���A��ʕ\������X�e�[�W�I���ֈȍ~�ł���悤�ɂȂ�܂ł̎���
    /// </summary>
    const float SCENE_CHANGE_TIME = 0.2f;

    // ��ȏ�u���b�N�𐶐����Ă��邩
    bool isBlocksCreated = false;

    // ���g���C
    private bool isStandBy = false;
    [SerializeField] private GameObject retryButton;
    [SerializeField] private GameObject standByBG;

    public void Initialize()
    {
        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage;
        else stageName = stageController.StageName;

        playPhaseUI.SetActive(false);
        resultWindow.gameObject.SetActive(false);
        missionWindow.gameObject.SetActive(false);
        matchUI.SetActive(false);
        matchRateText.enabled = false;
        retryButton.SetActive(false);
        standByBG.SetActive(false);

        toClearWindow = false;

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z�� �v�f���w��
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        lastAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = ShapeData.Shape.Empty;
                    mapObj[x, y, z] = null;
                    correctAnswer[x, y, z] = ShapeData.Shape.Empty;
                    lastAnswer[x, y, z] = ShapeData.Shape.Empty;
                }
            }
        }
    }

    /// <summary>
    /// ���s�t�F�[�Y�ڍs���̏���
    /// </summary>
    public void PhaseStart()
    {
        playPhaseUI.SetActive(true);

        // �����ƃv���C���[�̉𓚂��擾����
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    correctAnswer[x, y, z] = stageController.CorrectAnswer[x, y, z];
                    lastAnswer[x, y, z] = map[x, y, z];
                    map[x, y, z] = stageController.PlayerAnswer[x, y, z];
                }
            }
        }

        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;
        isSkip = false;
        IsFastForward = false;
        isBlocksCreated = false;
        isStandBy = false;

        AnswerInstance();
    }

    public void PhaseUpdate()
    {
        if (stageController.phase != StageController.PHASE.PLAY) return;

        if (isFalling)
        {
            Skip();
            FastForward();
            MeshCombine();
        }

        SceneChangeCheck();
    }

    /// <summary>
    /// ���s�t�F�[�Y�I��
    /// </summary>
    public void PhaseEnd()
    {
        StopAllCoroutines();

        matchRateText.enabled = false;
        matchUI.SetActive(false);
        playPhaseUI.SetActive(false);
        resultWindow.gameObject.SetActive(false);
        missionWindow.gameObject.SetActive(false);
        retryButton.SetActive(false);
        standByBG.SetActive(false);

        tapManager.LongTapReset();
        tapManager.DoubleTapReset();

        // �u���b�N��\��
        Transform children;
        for (int i = 0; i < combiners.Length; i++)
        {
            children = combiners[i].parent.GetComponentInChildren<Transform>();
            foreach (Transform obj in children)
            {
                obj.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        RemoveMeshes();
    }

    /// <summary>
    /// �������o�X�L�b�v
    /// </summary>
    void Skip()
    {
        tapManager.DoubleTap(() =>
        {
            if (isFalling)
            {
                isSkip = true;
                IsFastForward = false;
            }
        });
    }

    /// <summary>
    /// �������o������
    /// </summary>
    void FastForward()
    {
        tapManager.LongTap(() =>
            {
                IsFastForward = true;
                isSkip = false;
            }, () =>
            {
                IsFastForward = false;
            }, 0.5f);
    }

    void MeshCombine()
    {
        for(int i = 0; i < combiners.Length; i++)
        {
            if (combiners[i].isShapeUsed)
            {
                combiners[i].meshCombiner.SetParent(combiners[i].parent);
                combiners[i].meshCombiner.Combine(true, false);
            }
        }
    }

    void RemoveMeshes()
    {
        if(isBlocksCreated)
        {
            for (int i = 0; i < combiners.Length; i++)
            {
                if (combiners[i].isShapeUsed)
                {
                    combiners[i].meshCombiner.Remove();
                }
            }
        }
    }

    /// <summary>
    /// �𓚂̃��f���𐶐��A��v���̌v�Z
    /// </summary>
    void AnswerInstance()
    {
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                float matchRateTrout = 0; // 1�}�X���̈�v��

                float total = 0; // 1�}�X���̃u���b�N�̑���
                float c_total = 0; // ������1�}�X���̃u���b�N�̑���
                float excess = 0; // ���ߐ�
                float lack = 0; // �s����
                float diff = 0; // ���ᐔ

                bool hasAnything = false; // ����/�v���C���[�̉𓚂̂ǂ��炩�ŁA�����u����Ă���}�X�Ȃ�true

                for (int y = 0; y < mapSize.y; y++)
                {
                    // ����
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    UsedBlocksCheck(s);

                    // �O�̉𓚂ƈقȂ��Ă����琶��������
                    if (map[x, y, z] != lastAnswer[x, y, z])
                    {
                        if (mapObj[x, y, z]) Destroy(mapObj[x, y, z]);

                        //// �󔒕����͐������Ȃ�
                        if (map[x, y, z] != ShapeData.Shape.Empty)
                        {
                            Transform parent = GetCreateParent(s);

                            mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, parent);

                            if (!isBlocksCreated) isBlocksCreated = true;
                        }
                    }
                    else
                    {
                        if (mapObj[x, y, z])
                        {
                            mapObj[x, y, z].transform.position = pos;
                            mapObj[x, y, z].GetComponent<MeshRenderer>().enabled = true;

                            if (!isBlocksCreated) isBlocksCreated = true;
                        }
                    }

                    // �����ƃv���C���[�̉𓚂��ׂ�
                    if (map[x, y, z] != ShapeData.Shape.Empty)
                        total++;
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty)
                        c_total++;

                    // ���ߕ� �����ł͉����u����Ă��Ȃ��ꏊ�ɃI�u�W�F�N�g���u���ꂽ�ꍇ
                    if (correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                        excess++;

                    // �s���� �����ł͉����u����Ă���ꏊ�ɃI�u�W�F�N�g�������u����Ă��Ȃ������ꍇ
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] == ShapeData.Shape.Empty)
                        lack++;

                    // ���ᕪ �`���Ⴄ�ꍇ ���� ��łȂ��ꍇ
                    if (correctAnswer[x, y, z] != map[x, y, z] && correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                        diff++;

                    // ��v���v�Z�̕���@����/�v���C���[�̉𓚂̂ǂ��炩�ŉ����u����Ă���}�X��������
                    if (correctAnswer[x, y, z] != ShapeData.Shape.Empty || map[x, y, z] != ShapeData.Shape.Empty)
                        hasAnything = true;
                }

                if (hasAnything) hasBlockTrout++;

                // 1�}�X������̈�v�����v�Z
                if (excess > 0) // ���߂�����ꍇ
                {
                    matchRateTrout = (float)(1 - ((total - (c_total - diff)) / total));
                    // 1 - (( 1�}�X���̑��u���b�N�� ) - (( ������1�}�X���̃u���b�N�� ) - ���ᐔ ) / 1�}�X���̑��u���b�N�� )
                }
                else
                {
                    if (c_total > 0)
                        matchRateTrout = ((c_total - lack - diff) / c_total);
                    // (( ������1�}�X���̃u���b�N�� ) - ( �s���� ) - ( ���ᕪ )) / ( ������1�}�X���̃u���b�N�� )

                    // �����u���ĂȂ��ꍇ��0
                    if (c_total <= 0)
                        matchRateTrout = 0;
                }

                matchRateTroutTotal += matchRateTrout;
            }
        }

        StartCoroutine(Fall());
    }

    /// <summary>
    /// ����
    /// </summary>
    IEnumerator Fall()
    {
        if (stageController.IsTutorial)
        {
            yield return new WaitUntil(() => tutorial.EndPlayA);
        }

        isFalling = true;

        GameObject finalObj = mapObj[0, 0, 0]; // �Ō�̃I�u�W�F�N�g

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    if (mapObj[x, y, z])
                    {
                        ShapeObjects shapeObj = mapObj[x, y, z].GetComponent<ShapeObjects>();
                        shapeObj.TargetHeight = y;
                        shapeObj.FallSpeed = fallSpeed;
                        shapeObj.IsFall = true;
                        shapeObj.IsVibrate = true;

                        finalObj = mapObj[x, y, z];
                    }

                    if (!isSkip)
                        yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        if (finalObj)
        {
            var so = finalObj.GetComponent<ShapeObjects>();

            if (so != null)
            {
                // �Ō�̃I�u�W�F�N�g����������
                yield return new WaitUntil(() => !so.IsFall);
            }
        }

        MeshCombine();

        isFalling = false;

        if (stageController.IsTutorial)
        {
            yield return new WaitUntil(() => tutorial.EndPlayA);
            ResultDisp();
        }
        else Invoke("ResultDisp", fallToMatchdispTime);
    }

    /// <summary>
    /// ���U���g�\��
    /// </summary>
    void ResultDisp()
    {
        // �S�̂̈�v���Z�o
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);
        matchRateText.text = matchRate.ToString() + "%";
        matchUI.SetActive(true);

        StartCoroutine(ResultDirectionCoroutine());
    }

    WaitForSeconds wait05 = new WaitForSeconds(0.5f);

    /// <summary>
    /// ���U���g�\���@�R���[�`��
    /// </summary>
    /// <returns></returns>
    IEnumerator ResultDirectionCoroutine()
    {
        if (stageController.IsTutorial) yield return wait05;

        if (matchRate >= 100)
        {
            if (stageController.IsTutorial) tutorial.ToPlayB = true;

            completeText.SetActive(true);
            completeAnim.SetTrigger("Completed");
            completeEffect.Play();

            // 100�����o���I��������
            yield return new WaitUntil(() => completeAnim.GetCurrentAnimatorStateInfo(0).IsName("End"));

            // Tutorial�̏ꍇ�͐����I���܂ő҂�
            if (stageController.IsTutorial) yield return new WaitUntil(() => tutorial.EndPlayB);

            // ���U���g�\��
            resultWindow.gameObject.SetActive(true);
            missionScore.ResultDisp();

            vibration.PluralVibrate(2, (long)(clearVibrateTime * 1000));

            toClearWindow = true;
        }
        else
        {
            completeText.SetActive(false);
            matchRateText.enabled = true;
            StartCoroutine(MatchTextBlinking());
        }
    }

    const float UN_DISP_TIME = 0.3f;
    const float DISP_TIME = 0.5f;

    /// <summary>
    /// �p�[�Z���e�[�W�_�ŉ��o
    /// </summary>
    IEnumerator MatchTextBlinking()
    {
        WaitForSeconds disp = new WaitForSeconds(DISP_TIME);
        WaitForSeconds unDisp = new WaitForSeconds(UN_DISP_TIME);

        for (int i = 0; i < 2; i++)
        {
            yield return disp;
            matchRateText.enabled = false;
            yield return unDisp;
            matchRateText.enabled = true;
        }

        yield return disp;

        if (stageController.IsTutorial)
        {
            tutorial.ToPlayB = true;
            yield return new WaitUntil(() => tutorial.EndPlayB);
            yield return wait05;
            tutorial.ToPlayC = true;
            yield return new WaitUntil(() => tutorial.EndPlayC);
        }

        Failed();

        stageController.IsRetry = true;
    }

    /// <summary>
    /// �V�[���J�ڂł��邩�m�F����
    /// </summary>
    void SceneChangeCheck()
    {
        // ���U���g�\��������Ƀ^�b�v������
        if (resultWindow.DispEnd && toClearWindow && Input.touchCount >= 1)
        {
            // �ʏ�X�e�[�W���`���[�g���A���X�e�[�W�̏ꍇ�~�b�V�����B����ʂ�\��
            if (stageName.Contains("Stage") || stageName == "Tutorial")
            {
                resultWindow.gameObject.SetActive(false);
                missionWindow.gameObject.SetActive(true);

                // �\���I����
                if (missionWindow.DispEnd)
                {
                    // �`���[�g���A���̏ꍇ
                    if (stageController.IsTutorial)
                    {
                        StartCoroutine(SceneChangeForTutorial());
                    }

                    else
                    {
                        SceneChange();
                    }
                }
            }
            else
            {
                SceneChange();
            }
        }
    }

    /// <summary>
    /// ���s���̈�x����̏���
    /// </summary>
    void Failed()
    {
        sheatCreate.SheatDisp(true, true);
        retryButton.SetActive(true);
        cameraRotate.RotateReset(false);
    }

    IEnumerator SceneChangeForTutorial()
    {
        // 0.5�b�҂��Đ����E�B���h�E�\��
        // �S�E�B���h�E�̕\��������������0.5�b�҂��đJ��

        yield return wait05;

        tutorial.ToPlayC = true;

        yield return new WaitUntil(() => tutorial.EndPlayC);
        yield return wait05;

        stageController.IsClear = true;
    }

    /// <summary>
    /// �V�[���ړ�
    /// </summary>
    void SceneChange()
    {
        // 0.2�b�҂��đJ��
        StartCoroutine(DelayCoroutine(SCENE_CHANGE_TIME, () =>
        {
            stageController.IsClear = true;
        }));
    }

    /// <summary>
    /// �v���C���[�̉񓚂Ɏg�p����Ă���u���b�N�̎�ނ��m�F����
    /// </summary>
    void UsedBlocksCheck(ShapeData.Shape _shape)
    {
        if(_shape == ShapeData.Shape.Empty) return;

        for (int i = 0; i < combiners.Length; i++)
        {
            if(combiners[i].shape == _shape && !combiners[i].isShapeUsed)
                combiners[i].isShapeUsed = true;
        }
    }

    /// <summary>
    /// �����u���b�N�ɉ����Đ�������e�I�u�W�F�N�g��Ԃ�
    /// </summary>
    /// <param name="_shape"></param>
    Transform GetCreateParent(ShapeData.Shape _shape)
    {
        Transform parent = null;

        for(int i = 0; i < combiners.Length; i++)
        {
            if(combiners[i].shape == _shape)
            {
                parent = combiners[i].parent;
                break;
            }
        }

        return parent;
    }

    /// <summary>
    /// �U�����Ԏ擾
    /// </summary>
    /// <returns>[0]���ʏ�A[1]�������莞�̐U���̒���</returns>
    public float[] GetVibrateTime()
    {
        float[] t = { fallVibrateTime_Normal, fallVibrateTime_FastForward };
        return t;
    }

    /// <summary>
    /// �X�^���o�C���
    /// </summary>
    public void StandBy()
    {
        if (!isStandBy)
        {
            standByBG.SetActive(true);
            StartCoroutine(DelayCoroutine(0.01f, () =>
            {
                isStandBy = true;
            }));
        }
    }

    /// <summary>
    /// �X�^���o�C��ԉ���
    /// </summary>
    public void CancellStandBy()
    {
        standByBG.SetActive(false);
        isStandBy = false;
    }

    /// <summary>
    /// ���g���C
    /// </summary>
    public void Retry()
    {
        if (isStandBy)
        {
            stageController.Retry();
        }
    }
}