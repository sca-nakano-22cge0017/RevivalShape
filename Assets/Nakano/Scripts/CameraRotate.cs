using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

using UnityEngine.UI;

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

    Vector2[] dragRangeVertex = new Vector2[5]; // ����\�͈͂̒��S��4���_

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

    // ��]����
    bool didSwip; // �X���C�v�ŉ�]���������ǂ���
    bool isRestore = false;
    [SerializeField, Header("�X���C�v��]�����͈́@�ŏ�")] Vector2 rotateCancellRangeMin;
    [SerializeField, Header("�X���C�v��]�����͈́@�ő�")] Vector2 rotateCancellRangeMax;
    Vector2[] rotateCancellRangeVertex = new Vector2[5]; // �͈͂̒��S��4���_

    // �g�k
    float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] float vSpeed;

    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    float dragAdjust = 10;

    // �_�u���^�b�v
    bool isDoubleTapStart = false;
    float doubleTapTime = 0;
    bool is90Rotate = false;

    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ŏ�")] Vector2 cantDoubleTapRangeMin;
    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ő�")] Vector2 cantDoubleTapRangeMax;

    [SerializeField, Header("�_�u���^�b�v���̉�]�ɂ����鎞��")] float rotateTime = 1.0f;
    bool isRotating = false;
    bool isMoving_up2back = false;

    // �ړ�����
    enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    TeleportDir teleportDir = TeleportDir.NULL; // �_�u���^�b�v�ł̈ړ�����

    TeleportDir inputDir1 = TeleportDir.NULL; // ���ڂɓ��͂����ړ�����
    TeleportDir inputDir2 = TeleportDir.NULL; // ���ڂɓ��͂����ړ�����

    enum CameraPos { UP, BACK, RIGHT, FRONT, LEFT, NULL };
    CameraPos nowCameraPos = CameraPos.UP; // ���݂̃J�����ʒu
    CameraPos nextCameraPos = CameraPos.NULL; // ���̃J�����ʒu

    // 90�x���̕␳�ʒu�E��] ��ʒ[���_�u���^�b�v�ł����̈ʒu�Ɉړ�����
    Dictionary<CameraPos, Vector3> adjustPoint = new Dictionary<CameraPos, Vector3>();
    Dictionary<CameraPos, Vector3> adjustRot = new();

    // ���s�t�F�[�Y���̃J�����ʒu
    Vector3 play_Pos = new Vector3(0, 0, 0);
    [SerializeField, Header("���s�t�F�[�Y���̃J�����Ɛ������̂Ȃ��p�x")] float play_Angle;

    // �^�b�v/�X���C�v�\�͈͂�`��
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

    [SerializeField] Text debug;

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

        adjustRot[CameraPos.UP] = new Vector3(90, 0, 180);
        adjustRot[CameraPos.BACK] = new Vector3(181.79f, 180, 180);
        adjustRot[CameraPos.RIGHT] = new Vector3(181.79f, -90, 180);
        adjustRot[CameraPos.FRONT] = new Vector3(181.79f, 0, 180);
        adjustRot[CameraPos.LEFT] = new Vector3(181.79f, 90, 180);
    }

    void Update()
    {
        debug.text = "isRotating : " + isRotating + ", isRestore : " + isRestore + ", didSwip : " + didSwip;

        if (!CanRotate) return;

        // �I�u�W�F�N�g����ڂ𗣂��Ȃ��悤��
        if (isRestore)
        {
            if(nowCameraPos == CameraPos.UP) transform.LookAt(target, Vector3.back);
            else transform.LookAt(target, Vector3.up);
        }

        // ��]���o���̓_�u���^�b�v�s��
        if (!isRotating && !isRestore) DoubleTapRotate();

        BaseMove();
    }

    /// <summary>
    /// ��{�����@��]�A�g�k
    /// </summary>
    void BaseMove()
    {
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

                // ��]�O�̍��W��ۑ�
                lastPos = adjustPoint[nowCameraPos];

                // �V���O���^�b�v���A�X���C�v�ɂ���]��Ԃ����Ƃɖ߂�
                // ����͈͓��������珈������
                if (didSwip && stageController.TapOrDragRange(t.position, rotateCancellRangeMin, rotateCancellRangeMax))
                {
                    StartCoroutine(RotateRestore());
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                // �͈͊O����X���C�v�����Ƃ��p�̒���
                if (!stageController.TapOrDragRange(t.position))
                {
                    sPos = t.position;
                    lastTapPos = t.position;
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

                if(tx != 0 && ty != 0) didSwip = true;

                // �J�������猩�ď�E�E�����̃x�N�g������]���Ƃ��ĉ�]������
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up); // ���E����
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right); // �㉺����

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos; // ��]�ړ�
                pos += target;                          // ���s�ړ�


                if (pos.y >= -0.5f)
                {
                    transform.position = pos;
                    transform.LookAt(target, transform.up);
                }
                
                // �J�������T���v���̉��ɉ�荞�܂Ȃ��悤�ɒ���
                if (pos.y <= -0.495f && pos.y != -0.5f)
                {
                    CameraPosAdjust();
                }

                lastTapPos = t.position;
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
                        if (stageController.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax)) return;

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
                    if (stageController.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax)) return;

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
                    Rotate(CameraPos.BACK, CameraPos.FRONT, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.FRONT:
                    Rotate(CameraPos.UP, CameraPos.NULL, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.BACK:
                    Rotate(CameraPos.UP, CameraPos.NULL, CameraPos.LEFT, CameraPos.RIGHT);
                    break;
                case CameraPos.RIGHT:
                    Rotate(CameraPos.UP, CameraPos.NULL, CameraPos.BACK, CameraPos.FRONT);
                    break;
                case CameraPos.LEFT:
                    Rotate(CameraPos.UP, CameraPos.NULL, CameraPos.FRONT, CameraPos.BACK);
                    break;
            }

            is90Rotate = false;
            inputDir1 = TeleportDir.NULL;
            inputDir2 = TeleportDir.NULL;
        }
    }

    /// <summary>
    /// ���݂̃J�����ʒu�ɉ����ď㉺���E�����̃J�����ʒu��ݒ肵�ĉ�]������
    /// </summary>
    /// <param name="_up">��</param>
    /// <param name="_down">��</param>
    /// <param name="_right">�E</param>
    /// <param name="_left">��</param>
    void Rotate(CameraPos _up, CameraPos _down, CameraPos _right, CameraPos _left)
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

        if(nextCameraPos == CameraPos.NULL) return;

        StartCoroutine(Move());
    }

    /// <summary>
    /// �J�����ړ�
    /// </summary>
    /// <param name="nextPoint">�ړ���̍��W</param>
    IEnumerator Move()
    {
        // ���݈ʒu�ƖڕW�ʒu�������Ȃ瓮�����Ȃ�
        if(nowCameraPos == nextCameraPos) yield break;

        // �X���C�v�ŉ�]����Ă�����Ƃɖ߂�
        if (didSwip)
        {
            StartCoroutine(RotateRestore());
            yield return new WaitForSeconds(rotateTime);
        }

        isRotating = true;

        Vector3 nowRot = adjustRot[nowCameraPos];
        Vector3 nextRot = adjustRot[nextCameraPos];

        Vector3 angle; // ��]�� �I�C���[�p
        Vector3 relay = Vector3.zero; // ���p�ʒu

        // ��]�ʎ擾
        {
            if (nowCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.RIGHT)
            {
                angle = new Vector3(0, 90, 0);
            }
            else if (nowCameraPos == CameraPos.RIGHT && nextCameraPos == CameraPos.BACK)
            {
                angle = new Vector3(0, -90, 0);
            }
            else if (nowCameraPos == CameraPos.RIGHT && nextCameraPos == CameraPos.UP)
            {
                angle = new Vector3(0, 90, -91.79f);
            }
            else if (nowCameraPos == CameraPos.LEFT && nextCameraPos == CameraPos.UP)
            {
                angle = new Vector3(0, -90, 91.79f);
            }
            else if (nowCameraPos == CameraPos.UP && nextCameraPos == CameraPos.BACK)
            {
                angle = new Vector3(91.79f, -180, 0);
            }
            else if (nowCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.UP)
            {
                angle = new Vector3(91.79f, -180, 0);
            }
            else angle = nextRot - nowRot;
        }

        // ���p�n�_�擾
        {
            if (nowCameraPos == CameraPos.UP && nextCameraPos == CameraPos.BACK)
            {
                float x = target.x - range / 2;
                float z = target.z - range / 2;
                float y = Mathf.Sqrt(range * range - (x * x + z * z));

                relay = new Vector3(x, y, z);
            }
            else if (nowCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.UP)
            {
                float x = target.x + range / 2;
                float z = target.z - range / 2;
                float y = Mathf.Sqrt(range * range - (x * x + z * z));

                relay = new Vector3(x, y, z);
            }
            else
            {
                Vector3 m = Vector3.Lerp(adjustPoint[nowCameraPos], adjustPoint[nextCameraPos], 0.5f);
                relay = (m - target).normalized * range + target;

                if(nowCameraPos != CameraPos.UP && nextCameraPos != CameraPos.UP)
                {
                    relay.y = -0.5f;
                }
            }
        }

        // �ړ�
        transform.DOLocalPath(new[] { transform.position, relay, adjustPoint[nextCameraPos] }, rotateTime, PathType.CatmullRom).SetOptions(false);

        // ��]
        transform.DORotate(angle, rotateTime, RotateMode.WorldAxisAdd);

        yield return new WaitForSeconds(rotateTime);

        isRotating = false;
        nowCameraPos = nextCameraPos;
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

        didSwip = false;
        isRestore = false;
        tx = 0;
        ty = 0;
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
        if(isRotating || isRestore) return;

        // �X���C�v�ŉ�]����Ă���߂�
        if(didSwip) StartCoroutine(RotateRestore());

        // ��ɖ߂�
        nextCameraPos = CameraPos.UP;
        StartCoroutine(Move());

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
        lastPos = adjustPoint[CameraPos.UP];

        nowCameraPos = CameraPos.UP;

        StartCoroutine(RotateRestore());
    }

    /// <summary>
    /// �J�����̏����ݒ�
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
        adjustPoint.Add(CameraPos.UP, transform.position);
        for (int i = 1; i < 5; i++)
        {
            CameraPos c = (CameraPos)Enum.ToObject(typeof(CameraPos), i);
            adjustPoint.Add(c, Quaternion.Euler(0, i * 90, 0) * firstPos + target);
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

            var rect = new Rect(cantDoubleTapRangeMin.x, cantDoubleTapRangeMin.y, cantDoubleTapRangeMax.x - cantDoubleTapRangeMin.x, cantDoubleTapRangeMax.y - cantDoubleTapRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }
}
