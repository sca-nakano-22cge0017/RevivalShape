using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// �m�F�t�F�[�Y�@�J��������
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField, Header("�f�o�b�O�p")] SampleCheck sampleCheck;

    [SerializeField] StageController stageController;
    [SerializeField] Camera _camera;
    Vector3 mapSize;

    [SerializeField, Header("�I�u�W�F�N�g�Ƃ̋���")] float dir = 12;

    // �����ʒu
    Vector3 target;

    // �����ʒu
    float range; // �J�������ړ��ł��鋅�̔��a

    // �����̓J������Y���W��0�ɕ␳�ł���悤�ɂ��邽�߂̕ϐ�
    [SerializeField, Header("ajustAngle�x���ɕ␳�|�C���g��u��")] float adjustAngle;
    Vector3[] point; // �J�����ʒu�̕␳ ��苗���܂ŋߕt�����炱�̍��W�Ɉړ�������

    // ��]
    [SerializeField, Header("�h���b�O�̊��x")] float sensitivity;
    Vector2 sPos;    // �^�b�v�̎n�_
    Vector2 lastTapPos;  // �O�t���[���̃^�b�v�ʒu
    float wid, hei;  // �X�N���[���T�C�Y
    float tx, ty;
    Vector3 lastPos; // ��]�J�n�O�̍��W
    Quaternion lastRot; // ��]�J�n�O�̉�]
    bool isRestore = false;

    // �g�k
    float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] float vSpeed;

    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    float dragAdjust = 10;

    Vector2[] dragRangeVertex = new Vector2[5]; // �^�b�v�\�͈͂̒��S��4���_

    // �_�u���^�b�v
    bool isDoubleTapStart = false;
    float doubleTapTime = 0;
    bool is90Rotate = false;

    [SerializeField, Header("�_�u���^�b�v���̉�]�ɂ����鎞��")] float rotateTime = 1.0f;
    bool isNowRotate = false;

    // �ړ�����
    enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    TeleportDir teleportDir = TeleportDir.NULL;

    TeleportDir inputDir1 = TeleportDir.NULL; // ���ڂ̗���
    TeleportDir inputDir2 = TeleportDir.NULL; // ���ڂ̗���

    enum CameraPos { FRONT, BACK, RIGHT, LEFT, UP, NULL };
    CameraPos nowCameraPos = CameraPos.FRONT; // ���݂̃J�����ʒu
    CameraPos nextCameraPos = CameraPos.NULL; // ���̃J�����ʒu

    // 90�x���̕␳�ʒu ��ʒ[���_�u���^�b�v�ł����̈ʒu�Ɉړ�����
    Vector3[] adjustPoint_90 = new Vector3[5];

    // ���s�t�F�[�Y���̃J�����ʒu
    Vector3 play_Pos = new Vector3(0, 0, 0);
    [SerializeField, Header("���s�t�F�[�Y���̃J�����Ɛ������̂Ȃ��p�x")] float play_Angle;

    // �^�b�v/�X���C�v�\�͈͂�`��
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

    [SerializeField, Header("�X���C�v�͈̔� �ŏ�")] Vector2 dragRangeMin;
    [SerializeField, Header("�X���C�v�͈̔� �ő�")] Vector2 dragRangeMax;

    /// <summary>
    /// �J�����𓮂������ǂ���
    /// false�̂Ƃ��͓����Ȃ�
    /// </summary>
    public bool CanRotate { get; set; } = false;

    void Awake()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];

        // �^�b�v/�X���C�v�ł���͈͂��擾
        Vector2 rangeMin = stageController.GetTapOrDragRange()[0];
        Vector2 rangeMax = stageController.GetTapOrDragRange()[1];
        dragRangeVertex[0] = new Vector2((rangeMax.x + rangeMin.x) / 2, (rangeMax.y + rangeMin.y) / 2);
        dragRangeVertex[1] = new Vector2(rangeMin.x, rangeMin.y);
        dragRangeVertex[2] = new Vector2(rangeMax.x, rangeMin.y);
        dragRangeVertex[3] = new Vector2(rangeMin.x, rangeMax.y);
        dragRangeVertex[4] = new Vector2(rangeMax.x, rangeMax.y);

        Application.targetFrameRate = 120;
    }

    void Update()
    {
        if (!CanRotate) return;

        if(isNowRotate)
        {
            transform.LookAt(target);
        }
        else if (isRestore)
        {
            transform.LookAt(target, transform.up);
        }
        else
        {
            DoubleTapRotate();
        }

        // ��]
        if (Input.touchCount == 1 && !isRestore)
        {
            Touch t = Input.GetTouch(0);
            
            if (t.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t.position)) return;

                sPos = t.position;
                lastTapPos = t.position;

                LastPosSet();
            }
            else if (t.phase == TouchPhase.Moved)
            {
                // �͈͊O����X���C�v�����Ƃ��p�̒���
                if (!stageController.TapOrDragRange(t.position))
                {
                    sPos = t.position;
                    lastTapPos = t.position;

                    if (Mathf.Abs(tx) > 0.5f || Mathf.Abs(ty) > 0.5f)
                    {
                        StartCoroutine(RotateRestore());
                    }
                }

                // �X���C�v��
                tx = t.position.x - lastTapPos.x;
                ty = t.position.y - lastTapPos.y;

                // �ړ��ʂ����]�p�x�����߂�
                // �n�_����̈ړ��ʂ̐�Βl���AdragAjust��菬����������0�ɂ��A����/�����̈ړ��ɂ���
                if (Mathf.Abs(sPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(sPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / hei * sensitivity;

                // �J�������猩�ď�E�E�����̃x�N�g������]���Ƃ��ĉ�]������
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up); // ���E����
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right); // �㉺����

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos; // ��]�ړ�
                pos += target;                          // ���s�ړ�

                transform.position = pos;
                transform.LookAt(target, transform.up);

                // �J�������T���v���̉��ɉ�荞�܂Ȃ��悤�ɒ���
                if (pos.y <= 0.005f && pos.y != 0)
                {
                    CameraPosAdjust();
                }

                lastTapPos = t.position;
            }
            else if (t.phase == TouchPhase.Ended)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t.position)) return;

                // �X���C�v�ɂ���]��Ԃ����Ƃɖ߂�
                if (Mathf.Abs(tx) > 0.5f || Mathf.Abs(ty) > 0.5f)
                {
                    StartCoroutine(RotateRestore());
                }
            }
        }

        // �g�k
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // �͈͊O�͖���
            if (!stageController.TapOrDragRange(t1.position)) return;
            if (!stageController.TapOrDragRange(t2.position)) return;

            if (t2.phase == TouchPhase.Began)
            {
                // �^�b�v������_�Ԃ̋������擾
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = _camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);

                // �k��
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist);
                // �g��
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist);

                _camera.fieldOfView = vRatio;
                sDist = nDist;
            }
        }
    }

    void LastPosSet()
    {
        switch (nowCameraPos)
        {
            case CameraPos.UP:
                lastPos = adjustPoint_90[0];
                lastRot = Quaternion.Euler(90, 0, 180);
                break;
            case CameraPos.BACK:
                lastPos = adjustPoint_90[1];
                lastRot = Quaternion.Euler(0, 0, 0);
                break;
            case CameraPos.RIGHT:
                lastPos = adjustPoint_90[2];
                lastRot = Quaternion.Euler(180, -90, 180);
                break;
            case CameraPos.FRONT:
                lastPos = adjustPoint_90[3];
                lastRot = Quaternion.Euler(180, 0, 180);
                break;
            case CameraPos.LEFT:
                lastPos = adjustPoint_90[4];
                lastRot = Quaternion.Euler(180, 90, 180);
                break;
        }
    }

    /// <summary>
    /// �_�u���^�b�v��90�x��] �O�㍶�E�Ɉړ�����
    /// </summary>
    void DoubleTapRotate()
    {
        if (isDoubleTapStart)
        {
            doubleTapTime += Time.deltaTime;
            if(doubleTapTime < 0.2f)
            {
                if(Input.touchCount == 1)
                {
                    Touch t = Input.GetTouch(0);

                    if (Input.GetTouch(0).phase == TouchPhase.Began)
                    {
                        // �͈͊O�͖���
                        if (!stageController.TapOrDragRange(t.position)) return;

                        // ���S�����͏��O
                        if (stageController.TapOrDragRange(t.position, dragRangeMin, dragRangeMax)) return;

                        // ���͕�����ۑ�
                        inputDir2 = DoubleTapPosJudgement(t.position);

                        if (inputDir1 == inputDir2 && inputDir1 != TeleportDir.NULL && inputDir2 != TeleportDir.NULL)
                            teleportDir = inputDir1;

                        is90Rotate = true;

                        isDoubleTapStart = false;
                        doubleTapTime = 0.0f;
                    }
                }
            }
            else
            {
                isDoubleTapStart = false;
                doubleTapTime = 0.0f;

                inputDir1 = TeleportDir.NULL;
                inputDir2 = TeleportDir.NULL;
            }
        }
        else
        {
            if (Input.touchCount == 1)
            {
                Touch t = Input.GetTouch(0);

                if (t.phase == TouchPhase.Began)
                {
                    // �͈͊O�͖���
                    if (!stageController.TapOrDragRange(t.position)) return;

                    // ���S�����͏��O
                    if (stageController.TapOrDragRange(t.position, dragRangeMin, dragRangeMax)) return;

                    // ���͕�����ۑ�
                    inputDir1 = DoubleTapPosJudgement(t.position);

                    isDoubleTapStart = true;
                    doubleTapTime = 0.0f;
                }
            }
        }

        if (is90Rotate)
        {
            // ���݂̃J�����ʒu�ɉ����ď㉺���E�����߂�
            switch (nowCameraPos)
            {
                case CameraPos.UP:
                    SetNextCameraPos(CameraPos.BACK, CameraPos.FRONT, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.FRONT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.BACK:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.LEFT, CameraPos.RIGHT);
                    break;
                case CameraPos.RIGHT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.BACK, CameraPos.FRONT);
                    break;
                case CameraPos.LEFT:
                    SetNextCameraPos(CameraPos.UP, CameraPos.NULL, CameraPos.FRONT, CameraPos.BACK);
                    break;
            }

            is90Rotate = false;
            inputDir1 = TeleportDir.NULL;
            inputDir2 = TeleportDir.NULL;
        }
    }

    /// <summary>
    /// ���݂̃J�����ʒu�ɉ����ď㉺���E�����̃J�����ʒu�����߂�
    /// </summary>
    /// <param name="_up">��</param>
    /// <param name="_down">��</param>
    /// <param name="_right">�E</param>
    /// <param name="_left">��</param>
    void SetNextCameraPos(CameraPos _up, CameraPos _down, CameraPos _right, CameraPos _left)
    {
        switch (teleportDir)
        {
            case TeleportDir.UP:
                nextCameraPos = _up;
                break;
            case TeleportDir.DOWN:
                nextCameraPos = _down;
                break;
            case TeleportDir.RIGHT:
                nextCameraPos = _right;
                break;
            case TeleportDir.LEFT:
                nextCameraPos = _left;
                break;
            default:
                nextCameraPos = CameraPos.NULL;
                break;
        }

        switch (nextCameraPos)
        {
            case CameraPos.UP:
                if (nowCameraPos == CameraPos.BACK)
                {
                    float x = target.x + range / 2;
                    float z = target.z - range / 2;
                    float y = Mathf.Sqrt(range * range - (x * x + z * z));
                    Vector3 relay = new Vector3(x, y, z);

                    StartCoroutine(Move(adjustPoint_90[0], relay));
                }
                else StartCoroutine(Move(adjustPoint_90[0]));

                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.BACK:
                if(nowCameraPos == CameraPos.UP)
                {
                    float x = target.x - range / 2;
                    float z = target.z - range / 2;
                    float y = Mathf.Sqrt(range * range - (x * x + z * z));
                    Vector3 relay = new Vector3(x, y, z);

                    StartCoroutine(Move(adjustPoint_90[1], relay));
                }
                else StartCoroutine(Move(adjustPoint_90[1]));

                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.RIGHT:
                StartCoroutine(Move(adjustPoint_90[2]));
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.FRONT:
                StartCoroutine(Move(adjustPoint_90[3]));
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.LEFT:
                StartCoroutine(Move(adjustPoint_90[4]));
                nowCameraPos = nextCameraPos;
                break;

            case CameraPos.NULL:
                break;
        }
    }

    /// <summary>
    /// �J�����ړ�
    /// </summary>
    /// <param name="nextPoint">�ړ���̍��W</param>
    IEnumerator Move(Vector3 nextPoint)
    {
        isNowRotate = true;

        transform.DOLocalPath(new[] { transform.position, nextPoint }, rotateTime, PathType.CatmullRom).SetOptions(false);
        yield return new WaitForSeconds(rotateTime);

        isNowRotate = false;
    }

    /// <summary>
    /// ���p�n�_���K�v�ȏꍇ�̃J�����ړ�
    /// </summary>
    /// <param name="nextPoint">�ړ���̍��W</param>
    /// <param name="relayPoint">���p�n�_�̍��W</param>
    /// <returns></returns>
    IEnumerator Move(Vector3 nextPoint, Vector3 relayPoint)
    {
        isNowRotate = true;

        transform.DOLocalPath(new[] { transform.position, relayPoint, nextPoint }, rotateTime, PathType.CatmullRom).SetOptions(false);
        transform.DORotate(new Vector3(90, -180, 0), rotateTime, RotateMode.WorldAxisAdd);
        yield return new WaitForSeconds(rotateTime);

        isNowRotate = false;
    }

    /// <summary>
    /// �X���C�v�ɂ��J�����̈ړ������̈ʒu�ɖ߂�
    /// </summary>
    /// <returns></returns>
    IEnumerator RotateRestore()
    {
        isRestore = true;

        transform.DOLocalPath(new[] { transform.position, lastPos }, rotateTime, PathType.CatmullRom).SetOptions(false);
        yield return new WaitForSeconds(rotateTime);

        transform.rotation = lastRot;

        isRestore = false;
    }

    /// <summary>
    /// �^�b�v�ʒu���㉺���E�͈̔͂̂ǂ��ɂ��邩�𔻒肵�A�ړ�������Ԃ�
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns>������Ԃ�</returns>
    TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        // Input.mouseposition�͌��_���Ⴄ�̂Œ���
        _pos.y *= -1;
        _pos.y += hei;

        // �㕔��
        if (InsideOrOutsideJudgement(dragRangeVertex[2], dragRangeVertex[1], dragRangeVertex[0], _pos))
            return TeleportDir.UP;

        // ������
        if (InsideOrOutsideJudgement(dragRangeVertex[3], dragRangeVertex[4], dragRangeVertex[0], _pos))
            return TeleportDir.DOWN;

        // �E����
        if (InsideOrOutsideJudgement(dragRangeVertex[4], dragRangeVertex[2], dragRangeVertex[0], _pos))
            return TeleportDir.RIGHT;

        // ������
        if (InsideOrOutsideJudgement(dragRangeVertex[1], dragRangeVertex[3], dragRangeVertex[0], _pos))
            return TeleportDir.LEFT;

        return TeleportDir.NULL;
    }

    /// <summary>
    /// �O�ςɂ����O����
    /// </summary>
    /// <param name="_vertexLeft">��� �����̒��_</param>
    /// <param name="_vertexRight">��� �E���̒��_</param>
    /// <param name="_vertexTop">�㕔�̒��_</param>
    /// <param name="_judgePos">���肷��ʒu</param>
    /// <returns>�O�p�`�̓����ɂ����true</returns>
    bool InsideOrOutsideJudgement(Vector2 _vertexLeft, Vector2 _vertexRight, Vector2 _vertexTop, Vector2 _judgePos)
    {
        bool isInside = false;

        // �x�N�g��
        Vector3 leftToTop = _vertexTop - _vertexLeft;
        Vector3 leftToRight = _vertexRight - _vertexLeft;
        Vector3 rightToTop = _vertexTop - _vertexRight;
        Vector3 leftToJudge = _judgePos - _vertexLeft;
        Vector3 rightToJudge = _judgePos - _vertexRight;

        // �O�όv�Z
        Vector3 cross1 = Vector3.Cross(leftToJudge, leftToTop);
        Vector3 cross2 = Vector3.Cross(leftToRight, leftToJudge);
        Vector3 cross3 = Vector3.Cross(rightToTop, rightToJudge);

        // �O�ς�z�̒l�̕����������Ȃ�O�p�`�̓����ɓ_�����݂���
        if ((cross1.z > 0 && cross2.z > 0 && cross3.z > 0) || (cross1.z < 0 && cross2.z < 0 && cross3.z < 0))
            isInside = true;

        return isInside;
    }

    /// <summary>
    /// �J�����ʒu��␳����
    /// </summary>
    void CameraPosAdjust()
    {
        Vector3 nearPos = new Vector3(100, 100, 100);

        var pos = transform.position;

        foreach (var p in point)
        {
            var nearDiff = (pos - nearPos).magnitude;
            var diff = (pos - p).magnitude;

            if (nearDiff > diff) nearPos = p;
        }

        transform.position = nearPos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// �J�����̈ʒu�E��]��������Ԃɖ߂�
    /// </summary>
    public void RotateReset()
    {
        if(isNowRotate || isRestore) return;

        if(nowCameraPos == CameraPos.BACK)
        {
            float x = target.x + range / 2;
            float z = target.z - range / 2;
            float y = Mathf.Sqrt(range * range - (x * x + z * z));
            Vector3 relay = new Vector3(x, y, z);
            StartCoroutine(Move(adjustPoint_90[0], relay));
        }

        else StartCoroutine(Move(adjustPoint_90[0]));

        _camera.fieldOfView = vDefault;
    }

    /// <summary>
    /// ���s�t�F�[�Y���̃J�����ʒu�Ɉړ�
    /// </summary>
    public void PlayPhaseCamera()
    {
        play_Angle = 90 - play_Angle;

        float buttom = target.z + range * Mathf.Cos(play_Angle * Mathf.Deg2Rad);
        float height = range * Mathf.Sin(play_Angle * Mathf.Deg2Rad);

        play_Pos = new Vector3(target.x, buttom, height);

        transform.position = play_Pos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// ���s�t�F�[�Y����m�F�t�F�[�Y�ɖ߂����Ƃ��ɃJ�������ړ�
    /// </summary>
    public void Restore()
    {
        lastPos = adjustPoint_90[0];
        lastRot = Quaternion.Euler(90, 0, 180);

        nowCameraPos = CameraPos.UP;

        StartCoroutine(RotateRestore());
    }

    /// <summary>
    /// �J�����̒����ʒu��ݒ�
    /// </summary>
    public void TargetSet()
    {
        // �T�C�Y���
        if (stageController)
            mapSize = stageController.MapSize;
        else if (sampleCheck)
            mapSize = sampleCheck.MapSize;

        // �����ʒu�ݒ�
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);

        // �T���v���̃T�C�Y�ɉ����ăJ�����̏����ʒu�𒲐�
        transform.position = new Vector3(target.x, mapSize.x + dir, target.z);

        // ����
        transform.LookAt(target, transform.up);

        nowCameraPos = CameraPos.UP;
        nextCameraPos = CameraPos.NULL;

        // �␳�|�C���g�̐���
        range = (target - transform.position).magnitude;
        Vector3 firstPos = new Vector3(range, -0.5f, 0);
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }

        // 90�x���̕␳�|�C���g�̍쐬
        adjustPoint_90[0] = transform.position;
        for (int i = 1; i < adjustPoint_90.Length; i++)
        {
            adjustPoint_90[i] = Quaternion.Euler(0, i * 90, 0) * firstPos + target;
        }
    }

    private void OnGUI()
    {
        if (isDragRangeDraw)
        {
            var texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.red);
            texture.Apply();
            _texture = texture;

            var rect = new Rect(dragRangeMin.x, dragRangeMin.y, dragRangeMax.x - dragRangeMin.x, dragRangeMax.y - dragRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
