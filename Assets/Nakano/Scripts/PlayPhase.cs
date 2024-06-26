using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ���s�t�F�[�Y
/// </summary>
public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;
    Vibration vibration;

    // �e�I�u�W�F�N�g
    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map; // �z�u�f�[�^
    GameObject[,,] mapObj;   // GameObject�^�z��

    ShapeData.Shape[,,] correctAnswer; // ����
    ShapeData.Shape[,,] lastAnswer;    // �O�̉�

    bool isFalling = false;

    // �����X�L�b�v
    bool isSkip = false;
    int skipTapCount = 0;
    float skipTime = 0f;
    bool canJudgement = false;

    // ������
    public bool IsFastForward { get; private set; } = false;
    float longTapTime = 0;
    bool countStart = false;
    [field: SerializeField, Header("������̔{��")] public float FastForwardRatio { get; private set; }

    [SerializeField, Header("�������̐U���̒���(�b) �ʏ�")] float fallVibrateTime_Normal;
    [SerializeField, Header("�������̐U���̒���(�b) ������")] float fallVibrateTime_FastForward;
    [SerializeField, Header("�N���A���̐U���̒���(�b)")] float clearVibrateTime;

    [SerializeField, Header("�������x")] float fallSpeed;
    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ�����")] int fallPos;
    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ��Ԋu(sec)")] float fallInterval;
    [SerializeField, Tooltip("�I�u�W�F�N�g���S�ė������Ă����v���\���܂ł̎���(sec)")]
    float fallToMatchdispTime;

    // ��v���v�Z
    float matchRateTroutTotal = 0; // 1�}�X������̈�v���̘a
    int hasBlockTrout = 0; // 1�ȏ�u���b�N������}�X�̐�
    int matchRate = 0; // ��v��

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;

    /// <summary>
    /// �m�F����
    /// </summary>
    public bool IsDebug { get; private set; } = false;

    private void Awake()
    {
        // UI���������Ă���
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;

        vibration = GameObject.FindObjectOfType<Vibration>();
    }

    public void Initialize()
    {
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

    private void Update()
    {
        if (isFalling)
        {
            Skip();
            FastForward();
        }
    }

    /// <summary>
    /// �������o�X�L�b�v
    /// </summary>
    void Skip()
    {
        // 1��ڂ̃^�b�v
        if (Input.touchCount == 1 && skipTapCount == 0)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t.position)) return;

                skipTapCount++;
                canJudgement = true;
            }
        }

        if (canJudgement) skipTime += Time.deltaTime;

        if (skipTime <= 0.2f && skipTime >= 0.05f)
        {
            // 2��ڂ̃^�b�v
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    // �͈͊O�͖���
                    if (!stageController.TapOrDragRange(t.position)) return;

                    skipTapCount++;
                }
            }
        }
        else if (skipTime > 0.2f)
        {
            canJudgement = false;
            skipTapCount = 0;
            skipTime = 0f;
        }

        if (skipTapCount >= 2)
        {
            isSkip = true;
            IsFastForward = false;
            canJudgement = false;
            skipTime = 0f;
        }
        else isSkip = false;
    }

    /// <summary>
    /// �������o������
    /// </summary>
    void FastForward()
    {
        if (Input.touchCount == 1 && !IsFastForward)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t.position)) return;

                countStart = true;
                longTapTime = 0;
            }
        }

        if (countStart)
        {
            longTapTime += Time.deltaTime;
        }

        if (longTapTime >= 0.5f)
        {
            IsFastForward = true;
            isSkip = false;
        }

        if (Input.touchCount == 1 && (IsFastForward || countStart))
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Ended)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t.position)) return;

                longTapTime = 0;
                IsFastForward = false;
                countStart = false;
            }
        }
    }

    /// <summary>
    /// ���s�t�F�[�Y�ڍs���̏���
    /// </summary>
    public void PlayPhaseStart()
    {
        playPhaseUI.SetActive(true);

        // �����ƃv���C���[�̉𓚂��擾����
        correctAnswer = stageController.CorrectAnswer;
        map = stageController.PlayerAnswer;

        // ��v�� ������
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        matchRate = 0;

        AnswerInstance();

        isSkip = false;
        skipTapCount = 0;
        skipTime = 0f;
        canJudgement = false;

        IsFastForward = false;
        longTapTime = 0;
        countStart = false;
    }

    /// <summary>
    /// ���s�t�F�[�Y�I��
    /// </summary>
    public void PlayPhaseEnd()
    {
        // �u���b�N�폜
        Transform children = objParent.GetComponentInChildren<Transform>();
        foreach (Transform obj in children)
        {
            Destroy(obj.gameObject);
            //obj.gameObject.SetActive(false);
        }

        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);
        matchRateText.enabled = false;
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

                    // �󔒕����͐������Ȃ�
                    if (map[x, y, z] != ShapeData.Shape.Empty)
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

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
        isFalling = true;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    // �����u���b�N�Ȃ牉�o���΂�
                    if (map[x, y, z] == ShapeData.Shape.Alpha)
                    {
                        mapObj[x, y, z].transform.position = new Vector3(-x, y, z);
                        continue;
                    }

                    if(mapObj[x, y, z])
                    {
                        mapObj[x, y, z].GetComponent<ShapeObjects>().TargetHeight = y;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().FallSpeed = fallSpeed;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsFall = true;
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = true; // �U���I��
                    }

                    if (!isSkip)
                        yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        isFalling = false;
        
        yield return new WaitForSeconds(fallToMatchdispTime);
        MatchRateDisp();

        isSkip = false;
        IsFastForward = false;
    }

    /// <summary>
    /// �v���C���[�̉𓚂Ɛ����̈�v�����m�F�E�\��
    /// </summary>
    void MatchRateDisp()
    {
        // ��v���Z�o
        matchRate = (int)(matchRateTroutTotal / (float)hasBlockTrout * 100);

        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        // 100%�ŃN���A
        if (matchRate >= 100)
        {
            clearWindow.SetActive(true);

            vibration.PluralVibrate(2, (long)(clearVibrateTime * 1000));

            stageController.IsClear = true;
        }
        else
        {
            StartCoroutine(MatchTextBlinking());

            stageController.IsRetry = true;
        }
    }

    float unDispTime = 0.3f;
    float dispTime = 0.5f;

    /// <summary>
    /// �p�[�Z���e�[�W�_�ŉ��o
    /// </summary>
    IEnumerator MatchTextBlinking()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(dispTime);
            matchRateText.enabled = false;
            yield return new WaitForSeconds(unDispTime);
            matchRateText.enabled = true;
        }
    }

    /// <summary>
    /// �f�o�b�O�p�@���s�t�F�[�Y���ēx�s��
    /// </summary>
    public void DebugPlayRetry()
    {
        IsDebug = true;

        PlayPhaseEnd();
        stageController.ToPlayPhase();
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
}
