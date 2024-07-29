using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �m�F�t�F�[�Y�@�J��������
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField] private StageController stageController;
    [SerializeField] private Tutorial tutorial;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private Camera m_camera;
    private Vector3 mapSize;
    private float wid, hei;  // �X�N���[���T�C�Y

    [SerializeField, Header("�I�u�W�F�N�g�Ƃ̋���")] private float dir = 12;
    private Vector3 target; // �����ʒu
    private float range; // �J�������ړ��ł��鋅�̔��a

    // �����̓J������Y���W��0�ɕ␳�ł���悤�ɂ��邽�߂̕ϐ�
    [SerializeField, Header("ajustAngle�x���ɕ␳�|�C���g��u��")] private float adjustAngle;
    private Vector3[] point; // �J�����ʒu�̕␳ ��苗���܂ŋߕt�����炱�̍��W�Ɉړ�������
    const float MIN_Y = 0.0f; // y���W�̉���

    // ��]
    private float sensitivity;
    [SerializeField] private SensChenz sensChenger;
    private Vector2 startPos;    // �^�b�v�̎n�_
    private Vector2 lastTapPos;  // �O�t���[���̃^�b�v�ʒu
    private float tx, ty;
    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    private float dragAdjust = 10;

    // ��]����
    private bool didSwip; // �X���C�v�ŉ�]���������ǂ���
    private bool isRestoring = false;
    [SerializeField, Header("�X���C�v��]�����͈́@�ŏ�")] private Vector2 rotateCancellRangeMin;
    [SerializeField, Header("�X���C�v��]�����͈́@�ő�")] private Vector2 rotateCancellRangeMax;
    [SerializeField, Header("�X���C�v�ɂ���]������Ƃɖ߂�܂ł̎���")] private float restoreTime = 1.0f;

    private float restoreAngleY, restoreAngleXZ;
    private Vector3 restoreNormalVec = Vector3.right;

    // �g�k
    private float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    private float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] private float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] private float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] private float vSpeed;

    // �_�u���^�b�v
    private bool is90Rotate = false;
    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ŏ�")] private Vector2 cantDoubleTapRangeMin;
    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ő�")] private Vector2 cantDoubleTapRangeMax;
    [SerializeField, Header("�_�u���^�b�v���̉�]�ɂ����鎞��")] private float rotateTime = 1.0f;
    private bool isRotating = false;
    private Tween rotateTween = null;
    private bool isRotateStart = false;

    // �ړ�����
    private enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    private TeleportDir teleportDir = TeleportDir.NULL; // �ړ�����
    private TeleportDir inputDir1 = TeleportDir.NULL; // ���ڂɓ��͂����ړ�����
    private TeleportDir inputDir2 = TeleportDir.NULL; // ���ڂɓ��͂����ړ�����

    private enum CameraPos { UP, BACK, RIGHT, FRONT, LEFT, NULL };
    private CameraPos currentCameraPos = CameraPos.UP; // ���݂̃J�����ʒu
    private CameraPos nextCameraPos = CameraPos.NULL; // ���̃J�����ʒu

    // 90�x���̕␳�ʒu�E��] ��ʒ[���_�u���^�b�v�ł����̈ʒu�Ɉړ�����
    private Dictionary<CameraPos, Vector3> adjustPoint = new();
    private Dictionary<CameraPos, Quaternion> adjustQuaternion = new();

    // ���s�t�F�[�Y���̃J�����ʒu
    [SerializeField, Header("���s�t�F�[�Y���̃J�����Ɛ������̂Ȃ��p�x")] private float play_Angle;

    public void Initialize()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];

        adjustQuaternion[CameraPos.UP] = Quaternion.Euler(90, 0, 180);
        adjustQuaternion[CameraPos.BACK] = Quaternion.Euler(180, 180, 180);
        adjustQuaternion[CameraPos.RIGHT] = Quaternion.Euler(180, -90, 180);
        adjustQuaternion[CameraPos.FRONT] = Quaternion.Euler(180, 0, 180);
        adjustQuaternion[CameraPos.LEFT] = Quaternion.Euler(180, 90, 180);

        tx = 0;
        ty = 0;

        isRestoring = false;
        isRotating = false;
        is90Rotate = false;

        inputDir1 = TeleportDir.NULL;
        inputDir2 = TeleportDir.NULL;
        teleportDir = TeleportDir.NULL;

        currentCameraPos = CameraPos.UP;
        nextCameraPos = CameraPos.NULL;

        // �T�C�Y���
        if (stageController)
            mapSize = stageController.MapSize;

        // �����ʒu�ݒ�
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);

        // �T���v���̃T�C�Y�ɉ����ăJ�����̏����ʒu�𒲐�
        transform.position = new Vector3(target.x, mapSize.x + dir + MIN_Y, target.z);

        // �␳�|�C���g�̐���
        range = (target - transform.position).magnitude;

        Vector3 firstPos = new Vector3(range, MIN_Y, 0);
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

        // ����
        transform.LookAt(target, transform.up);

        SetSensitivity();
    }

    public void CameraUpdate()
    {
        TweenPauseControll();

        if(!stageController.IsPause)
        {
            if (isRestoring)
            {
                Restore();
                AdjustCameraToTarget();
            }

            if (!isRotating && !isRestoring)
            {
                Swip();
                Scaling();
                DoubleTap();
            }
        }
    }

    /// <summary>
    /// �X���C�v
    /// </summary>
    private void Swip()
    {
        if (stageController.IsTutorial && tutorial.MethodName != "CheckB" && !tutorial.TutorialCompleteByPhase) return;

        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);

            if (t.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!tapManager.TapOrDragRange(t.position)) return;

                startPos = t.position;
                lastTapPos = t.position;

                // �V���O���^�b�v���A�X���C�v�ɂ���]��Ԃ����Ƃɖ߂�
                // ����͈͓��������珈������
                if (didSwip &&
                    tapManager.TapOrDragRange(t.position, rotateCancellRangeMin, rotateCancellRangeMax) &&
                    ((stageController.IsTutorial && tutorial.TutorialCompleteByPhase) ||
                    !stageController.IsTutorial))
                {
                    //RestoreStart();
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                // �͈͊O����X���C�v�����Ƃ��p�̒���
                if (!tapManager.TapOrDragRange(t.position))
                {
                    startPos = t.position;
                    lastTapPos = t.position;
                }

                // �X���C�v��
                tx = t.position.x - lastTapPos.x;
                ty = t.position.y - lastTapPos.y;

                if (stageController.IsTutorial && tutorial.MethodName == "CheckB")
                {
                    ty = 0;
                }

                // �ړ��ʂ����]�p�x�����߂�
                // �n�_����̈ړ��ʂ̐�Βl���AdragAjust��菬����������0�ɂ��A����/�����̈ړ��ɂ���
                if (Mathf.Abs(startPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(startPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / hei * sensitivity;

                if (tx != 0 || ty != 0) didSwip = true;

                var up = transform.up;
                var right = transform.right;

                // �n�ʂ��ړ�����ꍇ�͉�]����ύX����
                if (transform.position.y <= MIN_Y && tx == 0)
                {
                    up = Vector3.up;
                    right = transform.rotation * Vector3.left;
                }

                // �J�������猩�ď�E�E�����̃x�N�g������]���Ƃ��ĉ�]������
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, up); // ���E����
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, right); // �㉺����

                var pos = transform.position;
                pos -= target;
                pos = angleAxisTB * angleAxisLR * pos; // ��]�ړ�
                pos += target; // ���s�ړ�

                if (pos.y >= MIN_Y)
                {
                    transform.position = pos;
                    transform.LookAt(target, transform.up);
                }
                else
                {
                    pos.y = MIN_Y;
                    transform.position = pos;
                    transform.LookAt(target, transform.up);

                    AdjustCameraToClosestPoint();
                }

                lastTapPos = t.position;
            }

            else if (t.phase == TouchPhase.Ended)
            {
                if (stageController.IsTutorial)
                {
                    // �E�h���b�O�Ŏ���
                    if (!tutorial.ToCheckB_2 && tx > 0) tutorial.ToCheckB_2 = true;

                    // ���h���b�O�Ŏ���
                    if (tutorial.ToCheckB_2 && !tutorial.ToCheckC && tx < 0) tutorial.ToCheckC = true;
                }
            }
        }
    }

    /// <summary>
    /// �g�k
    /// </summary>
    private void Scaling()
    {
        if (stageController.IsTutorial && !tutorial.TutorialCompleteByPhase) return;

        if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // �͈͊O�͖���
            if (!tapManager.TapOrDragRange(t1.position) || !tapManager.TapOrDragRange(t2.position)) return;

            if (t2.phase == TouchPhase.Began)
            {
                // �^�b�v������_�Ԃ̋������擾
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = m_camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);

                // �k��
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist);
                // �g��
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist);

                m_camera.fieldOfView = vRatio;
                sDist = nDist;

                transform.LookAt(target, transform.up);
            }
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g����ڂ𗣂��Ȃ��悤�ɒ����_��ݒ�
    /// </summary>
    private void AdjustCameraToTarget()
    {
        if (currentCameraPos == CameraPos.UP) transform.LookAt(target, Vector3.back);
        else transform.LookAt(target, Vector3.up);
    }

    /// <summary>
    /// �J�������I�u�W�F�N�g�̉��ɍs���Ȃ��悤�ɒ�������
    /// </summary>
    private void AdjustCameraToClosestPoint()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        var currentPosition = transform.position;

        foreach (var p in point)
        {
            if ((currentPosition - nearestPos).magnitude > (currentPosition - p).magnitude)
            {
                nearestPos = p;
            }
        }

        transform.position = nearestPos;
        transform.LookAt(target, transform.up);
    }

    /// <summary>
    /// �_�u���^�b�v��90�x��] �O�㍶�E�Ɉړ�����
    /// </summary>
    private void DoubleTap()
    {
        if (stageController.IsTutorial && tutorial.MethodName != "CheckD" && !tutorial.TutorialCompleteByPhase) return;

        tapManager.DoubleTap(
            () =>
            {
                Touch t = Input.GetTouch(0);

                // �͈͊O�͖���
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // ���͕�����ۑ�
                if (inputDir1 == TeleportDir.NULL)
                    inputDir1 = DoubleTapPosJudgement(t.position);
            },

            () =>
            {
                Touch t = Input.GetTouch(0);

                // �͈͊O�͖���
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // ���͕�����ۑ�
                if (inputDir2 == TeleportDir.NULL)
                    inputDir2 = DoubleTapPosJudgement(t.position);

                if (inputDir1 == inputDir2 && inputDir1 != TeleportDir.NULL && inputDir2 != TeleportDir.NULL)
                    teleportDir = inputDir1;
                else
                {
                    inputDir1 = TeleportDir.NULL;
                    inputDir2 = TeleportDir.NULL;
                }

                is90Rotate = true;
            },

            () =>
            {
                inputDir1 = TeleportDir.NULL;
                inputDir2 = TeleportDir.NULL;
            });

        if (is90Rotate)
        {
            if(stageController.IsTutorial)
            {
                if((tutorial.ToCheckD && !tutorial.ToCheckE && teleportDir == TeleportDir.RIGHT) || tutorial.TutorialCompleteByPhase)
                {
                    tutorial.IsCheckD = true;
                }
                else
                {
                    is90Rotate = false;
                    inputDir1 = TeleportDir.NULL;
                    inputDir2 = TeleportDir.NULL;
                    return;
                }
            }

            // ���݂̃J�����ʒu���猩���㉺���E�����߂�
            switch (currentCameraPos)
            {
                case CameraPos.UP:
                    RotateDirSet(CameraPos.BACK, CameraPos.FRONT, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.FRONT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.RIGHT, CameraPos.LEFT);
                    break;
                case CameraPos.BACK:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.LEFT, CameraPos.RIGHT);
                    break;
                case CameraPos.RIGHT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.BACK, CameraPos.FRONT);
                    break;
                case CameraPos.LEFT:
                    RotateDirSet(CameraPos.UP, CameraPos.NULL, CameraPos.FRONT, CameraPos.BACK);
                    break;
            }

            is90Rotate = false;
            inputDir1 = TeleportDir.NULL;
            inputDir2 = TeleportDir.NULL;
        }
    }

    /// <summary>
    /// �^�b�v�ʒu���㉺���E�͈̔͂̂ǂ��ɂ��邩�𔻒肵�A�ړ�������Ԃ�
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns>������Ԃ�</returns>
    private TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        string posName = tapManager.TapPosJudgement(_pos);
        TeleportDir dir;

        switch (posName)
        {
            case "up":
                dir = TeleportDir.UP;
                break;
            case "down":
                dir = TeleportDir.DOWN;
                break;
            case "right":
                dir = TeleportDir.RIGHT;
                break;
            case "left":
                dir = TeleportDir.LEFT;
                break;
            default:
                dir = TeleportDir.NULL;
                break;
        }

        return dir;
    }

    /// <summary>
    /// ���݂̃J�����ʒu�ɉ����ď㉺���E�����̃J�����ʒu��ݒ肵�ĉ�]������
    /// </summary>
    /// <param name="_up">��</param>
    /// <param name="_down">��</param>
    /// <param name="_right">�E</param>
    /// <param name="_left">��</param>
    private void RotateDirSet(CameraPos _up, CameraPos _down, CameraPos _right, CameraPos _left)
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

        if (nextCameraPos == CameraPos.NULL) return;

        StartCoroutine(Rotate90Degrees());
        isRotateStart = true;
    }

    /// <summary>
    /// 90�x��]
    /// </summary>
    /// <returns></returns>
    private IEnumerator Rotate90Degrees()
    {
        if (didSwip)
        {
            RestoreStart();
            yield return new WaitUntil(() => !isRestoring);
        }

        isRotating = true;
        var sequence = DOTween.Sequence();

        Vector3[] controllPoint = GetControllPoint(); // ����_�擾

        Tween move = transform.DOLocalPath(new[] { adjustPoint[nextCameraPos], controllPoint[0], controllPoint[1] }, rotateTime, PathType.CubicBezier).SetOptions(false);

        if (currentCameraPos != nextCameraPos)
        {
            if ((currentCameraPos != CameraPos.UP && nextCameraPos != CameraPos.UP) ||
                (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.FRONT) ||
                (currentCameraPos == CameraPos.FRONT && nextCameraPos == CameraPos.UP))
            {
                sequence
                    .Join(move)
                    .Join(transform.DORotateQuaternion(adjustQuaternion[nextCameraPos], rotateTime));
            }

            // �ど����/�E/��̂Ƃ��͉�]���@��ύX
            else
            {
                var angleXZ = new Vector3(0.0f, 0.0f, 0.0f); // X���AZ���̉�]
                var angleY = new Vector3(0.0f, 0.0f, 0.0f); // Y���̉�]

                // ��]�p�x�擾
                if (currentCameraPos == CameraPos.BACK && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(90.0f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.BACK)
                {
                    angleXZ = new Vector3(-90.0f, 0.0f, 0.0f);
                    angleY = new Vector3(0.0f, -180.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.RIGHT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -90.0f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.RIGHT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 90.0f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }

                else if (currentCameraPos == CameraPos.LEFT && nextCameraPos == CameraPos.UP)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, 90.0f);
                    angleY = new Vector3(0.0f, -90.0f, 0.0f);
                }
                else if (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.LEFT)
                {
                    angleXZ = new Vector3(0.0f, 0.0f, -90.0f);
                    angleY = new Vector3(0.0f, 90.0f, 0.0f);
                }

                // ��]
                if (nextCameraPos == CameraPos.UP)
                {
                    sequence
                        .Join(transform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd))
                        .Join(move)
                        .Append(transform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd));
                }
                else
                {
                    sequence
                        .Join(transform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd))
                        .Append(move)
                        .Join(transform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd));
                }
            }
        }

        sequence.OnComplete(() =>
        {
            currentCameraPos = nextCameraPos;
            didSwip = false;
            tx = 0;
            ty = 0;
            isRotating = false;
            isRestoring = false;
            rotateTween = null;

            if (stageController.IsTutorial && tutorial.MethodName == "CheckD")
            {
                tutorial.ToCheckE = true;
            }
        });

        rotateTween = sequence;
    }

    /// <summary>
    /// �X���C�v�ɂ��J�����̈ړ������̈ʒu�ɖ߂�
    /// </summary>
    /// <returns></returns>
    private void RestoreStart()
    {
        isRestoring = true;

        CalcRestoreVec();

        var from = transform.position - target;
        var to = adjustPoint[currentCameraPos] - target;

        // ���ʂɃx�N�g���𓊉e���AY����]/X�܂���Z����]�̊p�x�����߂�
        // XZ���ʂɓ��e
        var planeUpFrom = Vector3.ProjectOnPlane(from, Vector3.up);
        var planeUpTo = Vector3.ProjectOnPlane(to, Vector3.up);
        restoreAngleY = Vector3.SignedAngle(planeUpFrom, planeUpTo, Vector3.up);

        // XY/ZY���ʂɓ��e
        var planeRightFrom = Vector3.ProjectOnPlane(from, restoreNormalVec);
        var planeRightTo = Vector3.ProjectOnPlane(to, restoreNormalVec);
        restoreAngleXZ = Vector3.SignedAngle(planeRightFrom, planeRightTo, restoreNormalVec);

        restoreAngleXZ = Mathf.Clamp(restoreAngleXZ, -90, 90);
    }

    private void Restore()
    {
        // target�𒆐S�Ƃ��������v���̃x�N�g���𖈃t���[���v�Z������
        CalcRestoreVec();

        //Ray ray = new Ray(transform.position, restoreNormalVec);
        //Debug.DrawRay(ray.origin, ray.direction, Color.red, 100);

        var ratio = Time.deltaTime / restoreTime;

        // �s��̍쐬
        var angleAxisY = Quaternion.AngleAxis(restoreAngleY * ratio, Vector3.up);
        var angleAxisXZ = Quaternion.AngleAxis(restoreAngleXZ * ratio, restoreNormalVec);

        // �ړ�
        var pos = transform.position;
        pos -= target;
        pos = angleAxisXZ * angleAxisY * pos; // ��]�ړ�
        pos += target; // ���s�ړ�

        // ���ȉ��܂ŋߕt�������]�I��
        if ((adjustPoint[currentCameraPos] - transform.position).magnitude < 1.0f)
        {
            isRestoring = false;
            transform.position = adjustPoint[currentCameraPos];
            tx = 0;
            ty = 0;
            didSwip = false;

            if (stageController.IsTutorial && tutorial.IsCheckC)
            {
                tutorial.ToCheckD = true;
            }
        }
        else
        {
            if (pos.y < MIN_Y) pos.y = MIN_Y;

            transform.position = pos;
            transform.LookAt(target, transform.up);
        }
    }

    private void CalcRestoreVec()
    {
        // �I�u�W�F�N�g�O���ɂ���␳�ʒu�ƌ��݈ʒu�̂Ȃ��p�x�����߂�@�ˁ@Y����]�̗ʂ��Z�o
        var planeFrom = Vector3.ProjectOnPlane(adjustPoint[CameraPos.FRONT] - target, Vector3.up);
        var planeTo = Vector3.ProjectOnPlane(transform.position - target, Vector3.up);
        var angle = Vector3.SignedAngle(planeFrom, planeTo, Vector3.up);

        // target�𒆐S�Ƃ��������v���̃x�N�g��
        restoreNormalVec = (Quaternion.Euler(0, angle, 0) * Vector3.left).normalized;

        if (transform.position.y <= MIN_Y && adjustPoint[currentCameraPos].y <= MIN_Y) 
            restoreNormalVec = transform.rotation * Vector3.left;
    }

    private Vector3[] GetControllPoint()
    {
        Vector3 relay = Vector3.zero;
        Vector3[] point = new Vector3[2]; // ����_

        if (adjustPoint[currentCameraPos].x == adjustPoint[nextCameraPos].x) relay.x = adjustPoint[currentCameraPos].x;
        else relay.x = (Mathf.Abs(target.x - adjustPoint[currentCameraPos].x) >= 1f) ? adjustPoint[currentCameraPos].x : adjustPoint[nextCameraPos].x;

        if (adjustPoint[currentCameraPos].y == adjustPoint[nextCameraPos].y) relay.y = adjustPoint[currentCameraPos].y;
        else relay.y = (Mathf.Abs(target.y - (adjustPoint[currentCameraPos].y - MIN_Y)) >= 1f) ? adjustPoint[currentCameraPos].y : adjustPoint[nextCameraPos].y;

        if (adjustPoint[currentCameraPos].z == adjustPoint[nextCameraPos].z) relay.z = adjustPoint[currentCameraPos].z;
        else relay.z = (Mathf.Abs(target.z - adjustPoint[currentCameraPos].z) >= 1f) ? adjustPoint[currentCameraPos].z : adjustPoint[nextCameraPos].z;

        point[0] = Vector3.Lerp(adjustPoint[currentCameraPos], relay, 0.5f);
        point[1] = Vector3.Lerp(adjustPoint[nextCameraPos], relay, 0.5f);

        return point;
    }

    /// <summary>
    /// �J�����̈ʒu�E��]��������Ԃɖ߂�
    /// </summary>
    /// <param name="didGoThroughClosetPoint">�ł��߂��␳�|�C���g���o�R���邩</param>
    public void RotateReset(bool didGoThroughClosetPoint)
    {
        if (isRotating || isRestoring) return;

        if (currentCameraPos != CameraPos.UP && didGoThroughClosetPoint)
        {
            currentCameraPos = GetCameraToClosest90Point();
        }

        nextCameraPos = CameraPos.UP;
        m_camera.fieldOfView = vDefault;

        StartCoroutine(RotateResetCoroutine());
    }

    /// <summary>
    /// �J�����̈ʒu�E��]��������Ԃɖ߂��R���[�`��
    /// </summary>
    /// <returns></returns>
    private IEnumerator RotateResetCoroutine()
    {
        // �X���C�v���Ă�����߂�
        if (didSwip)
        {
            RestoreStart();
            yield return new WaitUntil(() => !isRestoring);
        }

        // �߂�؂�����A�I�u�W�F�N�g�㕔�ֈړ�
        if (currentCameraPos != CameraPos.UP)
        {
            StartCoroutine(Rotate90Degrees());
            isRotateStart = true;
        }
    }

    /// <summary>
    /// ���݂̃J�����ʒu����ł��߂�90�x���̕␳�|�C���g��Ԃ�
    /// </summary>
    /// <returns></returns>
    private CameraPos GetCameraToClosest90Point()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        CameraPos cameraPos = CameraPos.NULL;
        var currentPosition = transform.position;

        foreach (var p in adjustPoint)
        {
            if ((currentPosition - nearestPos).magnitude > (currentPosition - p.Value).magnitude)
            {
                nearestPos = p.Value;
                cameraPos = p.Key;
            }
        }

        return cameraPos;
    }

    /// <summary>
    /// ���s�t�F�[�Y���̃J�����ʒu�Ɉړ�
    /// </summary>
    public void ToPlayPhase()
    {
        StopAllCoroutines();

        transform.rotation = Quaternion.Euler(0, 0, 0);

        var angle = 90 - play_Angle;

        float buttom = target.z + range * Mathf.Cos(angle * Mathf.Deg2Rad);
        float height = range * Mathf.Sin(angle * Mathf.Deg2Rad);

        transform.position = new Vector3(target.x, buttom, height);
        transform.LookAt(target, transform.up);

        currentCameraPos = CameraPos.UP;
    }

    /// <summary>
    /// ���s�t�F�[�Y����m�F�t�F�[�Y�ɖ߂����Ƃ��ɃJ�������ړ�
    /// </summary>
    public void FromPlayPhase()
    {
        didSwip = true;
        RotateReset(false);
    }

    /// <summary>
    /// Tween�̈ꎞ��~/�ĊJ
    /// </summary>
    private void TweenPauseControll()
    {
        // 90�x��]�̈ꎞ��~/�ĊJ
        if (rotateTween != null)
        {
            if (!stageController.IsPause && isRotateStart)
            {
                isRotateStart = false;
                rotateTween.Play();
            }
            else if (stageController.IsPause && !isRotateStart)
            {
                isRotateStart = true;
                rotateTween.Pause();
            }
        }
    }

    /// <summary>
    /// ���x�ݒ�
    /// �ݒ��ʂ�����Ƃ��ɌĂ�
    /// </summary>
    public void SetSensitivity()
    {
        sensitivity = sensChenger.sensitivity;
    }

    /// <summary>
    /// �^�b�v�̏�Ԃ����Z�b�g
    /// </summary>
    public void TapReset()
    {
        tapManager.DoubleTapReset();
    }
}
