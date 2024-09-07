using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MathExtensions;

/// <summary>
/// �m�F�t�F�[�Y�@�J��������
/// </summary>
public class CameraRotate : MonoBehaviour
{
    private Transform thisTransform;

    [SerializeField] private Tutorial tutorial;
    [SerializeField] private StageController stageController;
    [SerializeField] private TapManager tapManager;
    [SerializeField] private Camera m_camera;
    private Vector3 mapSize;

    [SerializeField, Header("�I�u�W�F�N�g�Ƃ̋���")] private float dir = 12;
    private Vector3 target; // �����ʒu
    private float range;    // �J�������ړ����鋅�ʂ̔��a

    // �����̓J������Y���W��0�ɕ␳�ł���悤�ɂ��邽�߂̕ϐ�
    [SerializeField, Header("ajustAngle_Swip�x���ɕ␳�|�C���g��u��")] private float adjustAngle_Swip;
    private Vector3[] adjustPoint_Swip;  // �J�����ʒu�̕␳ ��苗���܂ŋߕt�����炱�̍��W�Ɉړ�������
    const float MIN_Y = 0.0f; // y���W�̉����@�����艺�ɃJ�����͂����Ȃ�

    // ��]
    private float sensitivity;
    [SerializeField] private SensChenz sensChenger;
    private Vector2 startPos;    // �^�b�v�̎n�_
    private Vector2 lastTapPos;  // �O�t���[���̃^�b�v�ʒu
    private float tx, ty;        // �X���C�v��
    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    private float dragAdjust = 10;

    // ��]����
    private bool didSwip;             // �X���C�v�ŉ�]���������ǂ���
    private bool isRestoring = false; // �X���C�v���ĉ�]������Ԃ���߂��Ă���Œ���
    [SerializeField, Header("�X���C�v��]�����͈́@�ŏ�")] private Vector2 rotateCancellRangeMin;
    [SerializeField, Header("�X���C�v��]�����͈́@�ő�")] private Vector2 rotateCancellRangeMax;
    [SerializeField, Header("�X���C�v�ɂ���]������Ƃɖ߂�܂ł̎���")] private float restoreTime = 1.0f;
    private Vector3 restoreTargetPos;            // �߂�ʒu
    private float restoreAngleY, restoreAngleXZ; // �߂�Ƃ��̉�]��

    // �g�k
    private float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    private float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] private float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] private float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] private float vSpeed;

    // �_�u���^�b�v
    private bool is90Rotate = false;    // ��]�ł��邩
    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ŏ�")] private Vector2 cantDoubleTapRangeMin;
    [SerializeField, Header("�_�u���^�b�v���O�͈́@�ő�")] private Vector2 cantDoubleTapRangeMax;
    [SerializeField, Header("�_�u���^�b�v���̉�]�ɂ����鎞��")] private float rotateTime = 1.0f;
    private bool isRotating = false;    // ��]����
    private Tween rotateTween = null;   // Tween �ꎞ��~�ł���悤�ɕێ�
    private bool isRotateStart = false; // ��]�J�n�̃t���O

    // 90�x���̕␳�ʒu�E��] ��ʒ[���_�u���^�b�v�ł����̈ʒu�Ɉړ�����
    private Dictionary<CameraPos, Vector3> adjustPoint_DoubleTap = new();
    private Dictionary<CameraPos, Quaternion> adjustQuaternion_DoubleTap = new();

    // �ړ�����
    private enum TeleportDir { UP, DOWN, RIGHT, LEFT, NULL }; // �l���� �㉺���E
    private TeleportDir teleportDir    = TeleportDir.NULL;    // �ړ�����
    private TeleportDir firstInputDir  = TeleportDir.NULL;    // ���ڂɓ��͂����ړ�����
    private TeleportDir secondInputDir = TeleportDir.NULL;    // ���ڂɓ��͂����ړ�����

    private enum CameraPos { UP, BACK, RIGHT, FRONT, LEFT, NULL }; // �ܕ����@�O�㍶�E��
    private CameraPos currentCameraPos = CameraPos.UP;             // ���݂̃J�����ʒu
    private CameraPos nextCameraPos    = CameraPos.NULL;          // ���̃J�����ʒu

    // ���s�t�F�[�Y���̃J�����ʒu
    [SerializeField, Header("���s�t�F�[�Y�� �J�����Ɛ������̂Ȃ��p�x")] private float play_Angle;

    WaitUntil untilRestored;

    public void Initialize()
    {
        // �L���b�V��
        thisTransform = GetComponent<Transform>();
        untilRestored = new WaitUntil(() => !isRestoring);

        adjustQuaternion_DoubleTap[CameraPos.UP]    = Quaternion.Euler(90, 0, 180);
        adjustQuaternion_DoubleTap[CameraPos.BACK]  = Quaternion.Euler(180, 180, 180);
        adjustQuaternion_DoubleTap[CameraPos.RIGHT] = Quaternion.Euler(180, -90, 180);
        adjustQuaternion_DoubleTap[CameraPos.FRONT] = Quaternion.Euler(180, 0, 180);
        adjustQuaternion_DoubleTap[CameraPos.LEFT]  = Quaternion.Euler(180, 90, 180);

        tx = 0;
        ty = 0;

        isRestoring = false;
        isRotating = false;
        is90Rotate = false;

        firstInputDir  = TeleportDir.NULL;
        secondInputDir = TeleportDir.NULL;
        teleportDir    = TeleportDir.NULL;

        currentCameraPos = CameraPos.UP;
        nextCameraPos    = CameraPos.NULL;

        // ���x�ݒ�
        SetSensitivity();

        // �T�C�Y���
        if (stageController) mapSize = stageController.MapSize;

        // �����ʒu�ݒ�
        target = mapSize / 2;
        target.x *= -1;                        // X���̕����Ɣ��Ό����ɃI�u�W�F�N�g�������Ă���̂�-1��������
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f); // �Y����␳

        // �T���v���̃T�C�Y�ɉ����ăJ�����̏����ʒu�𒲐�
        thisTransform.position = new Vector3(target.x, mapSize.x + dir + MIN_Y, target.z);

        // �J�������������ʂ̔��a���Z�o
        range = (target - thisTransform.position).magnitude;

        // �X���C�v�̕␳�|�C���g�̐���
        Vector3 firstPos = new Vector3(range, MIN_Y, 0);
        adjustPoint_Swip = new Vector3[(int)(360 / adjustAngle_Swip)];
        for (int i = 0; i < adjustPoint_Swip.Length; i++)
        {
            adjustPoint_Swip[i] = Quaternion.Euler(0, i * adjustAngle_Swip, 0) * firstPos + target;
        }

        // �_�u���^�b�v�ňړ�����|�C���g�̍쐬
        adjustPoint_DoubleTap.Add(CameraPos.UP, thisTransform.position);
        for (int i = 1; i < 5; i++)
        {
            CameraPos c = (CameraPos)Enum.ToObject(typeof(CameraPos), i);
            adjustPoint_DoubleTap.Add(c, Quaternion.Euler(0, i * 90, 0) * firstPos + target);
        }

        // ����
        thisTransform.LookAt(target, thisTransform.up);
    }

    private void Update()
    {
        TweenPauseControll();
    }

    public void CameraUpdate()
    {
        if(!stageController.IsPause)
        {
            if (isRestoring)
            {
                Restore();
                AdjustCameraToTarget();
            }

            // ��]���o���͑���s��
            if (!isRotating && !isRestoring)
            {
                Swip();
                Scaling();
                DoubleTap();
            }
        }
    }

    bool isLeftSwip = false, isRightSwip = false;

    /// <summary>
    /// �X���C�v
    /// </summary>
    private void Swip()
    {
        if (stageController.IsTutorial)
        {
            if (tutorial.MethodName != "CheckB" && tutorial.MethodName != "CheckC2" && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete)
                return;
        }
        
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
                    ((stageController.IsTutorial && (tutorial.TutorialCompleteByPhase || tutorial.MethodName == "CheckC2")) ||
                    !stageController.IsTutorial))
                {
                    restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
                    RestoreStart();
                }
            }
            else if (t.phase == TouchPhase.Moved)
            {
                if (tutorial.MethodName == "CheckC2") return;

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

                    if (tx > 5)  isRightSwip = true;
                    if (tx < -5) isLeftSwip = true;
                }

                // �ړ��ʂ����]�p�x�����߂�
                // �n�_����̈ړ��ʂ̐�Βl���AdragAjust��菬����������0�ɂ��A����/�����̈ړ��ɂ���
                if (Mathf.Abs(startPos.x - t.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / Screen.width * sensitivity;

                if (Mathf.Abs(startPos.y - t.position.y) < dragAdjust) ty = 0;
                float deltaAngleTB = -ty / Screen.height * sensitivity;

                if (tx != 0 || ty != 0) didSwip = true;

                var up = thisTransform.up;
                var right = thisTransform.right;

                // �n�ʂ��ړ�����ꍇ�͉�]����ύX����
                if (thisTransform.position.y <= MIN_Y && ty == 0)
                {
                    up = Vector3.up;
                    right = thisTransform.rotation * Vector3.left;
                }

                // �J�������猩�ď�E�E�����̃x�N�g������]���Ƃ��ĉ�]������
                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, up); // ���E����
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, right); // �㉺����

                var pos = thisTransform.position;
                pos -= target;
                pos = angleAxisTB * angleAxisLR * pos; // ��]�ړ�
                pos += target; // ���s�ړ�

                if (pos.y >= MIN_Y)
                {
                    thisTransform.position = pos;
                    thisTransform.LookAt(target, thisTransform.up);
                }
                else
                {
                    pos.y = MIN_Y;
                    thisTransform.position = pos;
                    thisTransform.LookAt(target, thisTransform.up);

                    AdjustCameraToClosestPoint();
                }

                lastTapPos = t.position;
            }

            else if (t.phase == TouchPhase.Ended)
            {
                if (stageController.IsTutorial)
                {
                    // �E�h���b�O�Ŏ���
                    if (tutorial.IsCheckB_1 && !tutorial.ToCheckB_2 && isRightSwip) tutorial.ToCheckB_2 = true;

                    // ���h���b�O�Ŏ���
                    if (tutorial.IsCheckB_2 && !tutorial.ToCheckC && isLeftSwip) tutorial.ToCheckC = true;
                }
            }
        }
    }

    /// <summary>
    /// �g�k
    /// </summary>
    private void Scaling()
    {
        if (stageController.IsTutorial && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete) return;

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

                thisTransform.LookAt(target, thisTransform.up);
            }
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g����ڂ𗣂��Ȃ��悤�ɒ����_��ݒ�
    /// </summary>
    private void AdjustCameraToTarget()
    {
        if (currentCameraPos == CameraPos.UP)
        {
            thisTransform.LookAt(target, Vector3.back);
        }
        else
        {
            thisTransform.LookAt(target, Vector3.up);
        }
    }

    /// <summary>
    /// �J�������I�u�W�F�N�g�̉��ɍs���Ȃ��悤�ɒ�������
    /// </summary>
    private void AdjustCameraToClosestPoint()
    {
        Vector3 nearestPos = new Vector3(100, 100, 100);
        var currentPosition = thisTransform.position;

        // �����Ƃ��߂��|�C���g��T��
        foreach (var p in adjustPoint_Swip)
        {
            if ((currentPosition - nearestPos).sqrMagnitude > (currentPosition - p).sqrMagnitude)
            {
                nearestPos = p;
            }
        }

        thisTransform.position = nearestPos;
        thisTransform.LookAt(target, thisTransform.up);
    }

    /// <summary>
    /// �_�u���^�b�v��90�x��] �O�㍶�E�Ɉړ�����
    /// </summary>
    private void DoubleTap()
    {
        if (stageController.IsTutorial)
        {
            if (tutorial.MethodName != "CheckD" && !tutorial.TutorialCompleteByPhase && !tutorial.IsTutorialComplete)
                return;
        }

        tapManager.DoubleTap(
            () =>
            {
                // ���ڂ̃^�b�v
                Touch t = Input.GetTouch(0);

                // �͈͊O�͖���
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // ���͕�����ۑ�
                if (firstInputDir == TeleportDir.NULL)
                    firstInputDir = DoubleTapPosJudgement(t.position);
            },

            () =>
            {
                // ���ڂ̃^�b�v
                Touch t = Input.GetTouch(0);

                // �͈͊O�͖���
                if (!tapManager.TapOrDragRange(t.position) ||
                    tapManager.TapOrDragRange(t.position, cantDoubleTapRangeMin, cantDoubleTapRangeMax))
                    return;

                // ���͕�����ۑ�
                if (secondInputDir == TeleportDir.NULL)
                    secondInputDir = DoubleTapPosJudgement(t.position);

                if (firstInputDir == secondInputDir && firstInputDir != TeleportDir.NULL && secondInputDir != TeleportDir.NULL)
                    teleportDir = firstInputDir;
                else
                {
                    firstInputDir = TeleportDir.NULL;
                    secondInputDir = TeleportDir.NULL;
                }

                is90Rotate = true;
            },

            () =>
            {
                // ���ԓ��ɓ�x�ڂ̃^�b�v�����������ꍇ
                firstInputDir = TeleportDir.NULL;
                secondInputDir = TeleportDir.NULL;
            });

        if (is90Rotate)
        {
            if (stageController.IsTutorial && !tutorial.IsTutorialComplete)
            {
                // �`���[�g���A�� �m�F�t�F�[�YD
                // �E�����ɂ̂݃_�u���^�b�v�ňړ��\
                // �ړ�������`���[�g���A����i�߂�
                if ((tutorial.ToCheckD && !tutorial.ToCheckE && teleportDir == TeleportDir.RIGHT) || tutorial.TutorialCompleteByPhase)
                {
                    tutorial.IsCheckD = true;
                }
                else
                {
                    is90Rotate = false;
                    firstInputDir = TeleportDir.NULL;
                    secondInputDir = TeleportDir.NULL;
                    return;
                }
            }

            // ���݂̃J�����ʒu���猩���㉺���E�����߂�
            // �J�������T���v���̏㑤�ɂ���Ƃ��A�w��x�̓T���v������A�w���x�̓T���v���O���ɂȂ�
            CameraPos up    = CameraPos.NULL, 
                      down  = CameraPos.NULL, 
                      left  = CameraPos.NULL, 
                      right = CameraPos.NULL;

            switch (currentCameraPos)
            {
                case CameraPos.UP:
                    up    = CameraPos.BACK;
                    down  = CameraPos.FRONT;
                    right = CameraPos.RIGHT;
                    left  = CameraPos.LEFT;
                    break;

                case CameraPos.FRONT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.RIGHT;
                    left  = CameraPos.LEFT;
                    break;

                case CameraPos.BACK:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.LEFT;
                    left  = CameraPos.RIGHT;
                    break;

                case CameraPos.RIGHT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.BACK;
                    left  = CameraPos.FRONT;
                    break;

                case CameraPos.LEFT:
                    up    = CameraPos.UP;
                    down  = CameraPos.NULL;
                    right = CameraPos.FRONT;
                    left  = CameraPos.BACK;
                    break;
            }

            if(currentCameraPos != CameraPos.UP)
            {
                if (didSwip && tx > 0)
                    right = currentCameraPos;

                else if (didSwip && tx < 0)
                    left  = currentCameraPos;

                else if (didSwip && ty > 0)
                    down  = currentCameraPos;
            }
            else
            {
                if (didSwip && ty > 0)
                    up = currentCameraPos;
                else if (didSwip && ty < 0)
                    down = currentCameraPos;
            }

            RotateDirSet(up, down, right, left);

            is90Rotate = false;
            firstInputDir = TeleportDir.NULL;
            secondInputDir = TeleportDir.NULL;
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
            // �J�������T���v���̑O�㍶�E�Ɉړ�������Ƃ�
            if ((adjustPoint_DoubleTap[currentCameraPos].y == MIN_Y && adjustPoint_DoubleTap[nextCameraPos].y == MIN_Y) ||
                (tx == 0 && ty < 0 && currentCameraPos == CameraPos.FRONT))
            {
                // �J������DoTween���g�킸�Ɉړ�������
                restoreTargetPos = adjustPoint_DoubleTap[nextCameraPos];
                RestoreStart();
                yield return untilRestored;

                if(nextCameraPos == CameraPos.UP)
                    thisTransform.LookAt(target, Vector3.back);

                currentCameraPos = nextCameraPos;
                yield break;
            }

            else
            {
                restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
                RestoreStart();
                yield return untilRestored;
            }
        }

        isRotating = true;
        var sequence = DOTween.Sequence();

        Vector3[] controllPoint = GetControllPoint(); // ����_�擾

        Tween move = thisTransform.DOLocalPath(new[] { adjustPoint_DoubleTap[nextCameraPos], controllPoint[0], controllPoint[1] }, rotateTime, PathType.CubicBezier).SetOptions(false);

        if (currentCameraPos != nextCameraPos)
        {
            if ((currentCameraPos != CameraPos.UP && nextCameraPos != CameraPos.UP) ||
                (currentCameraPos == CameraPos.UP && nextCameraPos == CameraPos.FRONT) ||
                (currentCameraPos == CameraPos.FRONT && nextCameraPos == CameraPos.UP))
            {
                sequence
                    .Join(move)
                    .Join(thisTransform.DORotateQuaternion(adjustQuaternion_DoubleTap[nextCameraPos], rotateTime));
            }

            // �ど����/�E/��̂Ƃ��͉�]�p�x���w�肵�ĉ�]������
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
                        .Join(thisTransform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd))
                        .Join(move)
                        .Append(thisTransform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd));
                }
                else
                {
                    sequence
                        .Join(thisTransform.DORotate(angleY, rotateTime / 2.0f, RotateMode.WorldAxisAdd))
                        .Append(move)
                        .Join(thisTransform.DORotate(angleXZ, rotateTime, RotateMode.WorldAxisAdd));
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

        Vector3 restoreNormalVec = CalcRestoreVec();

        var from = thisTransform.position - target;
        var to = restoreTargetPos - target;

        // ���ʂɃx�N�g���𓊉e���AY����]/X�܂���Z����]�̊p�x�����߂�
        restoreAngleY  = ProjectionAngle(from, to, Vector3.up);       // XZ���ʂɓ��e
        restoreAngleXZ = ProjectionAngle(from, to, restoreNormalVec); // XY/ZY���ʂɓ��e

        if (restoreAngleXZ > 90)
        {
            restoreAngleXZ = (180 - restoreAngleXZ) * -1;
        }
    }

    private void Restore()
    {
        // target�𒆐S�Ƃ��������v���̃x�N�g���𖈃t���[���v�Z������
        Vector3 restoreNormalVec = CalcRestoreVec();

        var ratio = Time.deltaTime / restoreTime;

        // �s��̍쐬
        var angleAxisY  = Quaternion.AngleAxis(restoreAngleY * ratio,  Vector3.up);
        var angleAxisXZ = Quaternion.AngleAxis(restoreAngleXZ * ratio, restoreNormalVec);

        // �ړ�
        var pos = thisTransform.position;
        pos -= target;
        pos = angleAxisXZ * angleAxisY * pos; // ��]�ړ�
        pos += target; // ���s�ړ�

        // ���ȉ��܂ŋߕt�������]�I��
        if ((restoreTargetPos - thisTransform.position).sqrMagnitude < 1.0f)
        {
            isRestoring = false;
            thisTransform.position = restoreTargetPos;
            tx = 0;
            ty = 0;
            didSwip = false;

            if (stageController.IsTutorial && tutorial.MethodName == "CheckC2")
            {
                tutorial.ToCheckD = true;
            }
        }
        else
        {
            if (pos.y < MIN_Y) pos.y = MIN_Y;
            thisTransform.position = pos;
        }
    }

    private Vector3 CalcRestoreVec()
    {
        Vector3 vec = Vector3.zero;

        // �I�u�W�F�N�g�O���ɂ���␳�ʒu�ƌ��݈ʒu�̂Ȃ��p�x�����߂�@�ˁ@Y����]�̗ʂ��Z�o
        var planeFrom = Vector3.ProjectOnPlane(adjustPoint_DoubleTap[CameraPos.FRONT] - target, Vector3.up);
        var planeTo = Vector3.ProjectOnPlane(thisTransform.position - target, Vector3.up);
        var angle = Vector3.SignedAngle(planeFrom, planeTo, Vector3.up);

        if (thisTransform.position.y <= MIN_Y && restoreTargetPos.y <= MIN_Y)
        {
            vec = thisTransform.rotation * Vector3.left;
        }
        else
        {
            // target�𒆐S�Ƃ��������v���̃x�N�g��
            vec = (Quaternion.Euler(0, angle, 0) * Vector3.left).normalized;
        }

        return vec;
    }

    private Vector3[] GetControllPoint()
    {
        Vector3 relay = Vector3.zero;
        Vector3[] controllPoint = new Vector3[2]; // ����_

        if (adjustPoint_DoubleTap[currentCameraPos].x == adjustPoint_DoubleTap[nextCameraPos].x) relay.x = adjustPoint_DoubleTap[currentCameraPos].x;
        else relay.x = (Mathf.Abs(target.x - adjustPoint_DoubleTap[currentCameraPos].x) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].x : adjustPoint_DoubleTap[nextCameraPos].x;

        if (adjustPoint_DoubleTap[currentCameraPos].y == adjustPoint_DoubleTap[nextCameraPos].y) relay.y = adjustPoint_DoubleTap[currentCameraPos].y;
        else relay.y = (Mathf.Abs(target.y - (adjustPoint_DoubleTap[currentCameraPos].y - MIN_Y)) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].y : adjustPoint_DoubleTap[nextCameraPos].y;

        if (adjustPoint_DoubleTap[currentCameraPos].z == adjustPoint_DoubleTap[nextCameraPos].z) relay.z = adjustPoint_DoubleTap[currentCameraPos].z;
        else relay.z = (Mathf.Abs(target.z - adjustPoint_DoubleTap[currentCameraPos].z) >= 1f) ? adjustPoint_DoubleTap[currentCameraPos].z : adjustPoint_DoubleTap[nextCameraPos].z;

        controllPoint[0] = Vector3.Lerp(adjustPoint_DoubleTap[currentCameraPos], relay, 0.5f);
        controllPoint[1] = Vector3.Lerp(adjustPoint_DoubleTap[nextCameraPos], relay, 0.5f);

        return controllPoint;
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
            restoreTargetPos = adjustPoint_DoubleTap[currentCameraPos];
            RestoreStart();
            yield return untilRestored;
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
        var currentPosition = thisTransform.position;

        foreach (var p in adjustPoint_DoubleTap)
        {
            if ((currentPosition - nearestPos).sqrMagnitude > (currentPosition - p.Value).sqrMagnitude)
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

        thisTransform.rotation = Quaternion.Euler(0, 0, 0);

        var angle = 90 - play_Angle;

        float buttom = target.z + range * Mathf.Cos(angle * Mathf.Deg2Rad);
        float height = range * Mathf.Sin(angle * Mathf.Deg2Rad);

        thisTransform.position = new Vector3(target.x, buttom, height);
        thisTransform.LookAt(target, thisTransform.up);

        didSwip = true;
        currentCameraPos = CameraPos.UP;

        TapReset();
    }

    /// <summary>
    /// ���s�t�F�[�Y����m�F�t�F�[�Y�ɖ߂����Ƃ��ɃJ�������ړ�
    /// </summary>
    public void FromPlayPhase()
    {
        //currentCameraPos = CameraPos.UP;
        //didSwip = true;
        RotateReset(false);
        TapReset();
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
        if (sensitivity == 0) sensitivity = 80;
    }

    /// <summary>
    /// �^�b�v�̏�Ԃ����Z�b�g
    /// </summary>
    public void TapReset()
    {
        tapManager.DoubleTapReset();
    }
}
