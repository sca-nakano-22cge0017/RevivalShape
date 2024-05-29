using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �m�F�t�F�[�Y�@�J��������
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField, Header("�f�o�b�O�p")] SampleCheck sampleCheck;

    [SerializeField] StageController stageController;
    [SerializeField] Camera _camera;
    Vector3 mapSize;

    // �����ʒu
    Vector3 target;

    // �����ʒu
    Vector3 defaultPos;
    Quaternion defaultRot;

    // �����̓J������Y���W��0�ɕ␳�ł���悤�ɂ��邽�߂̕ϐ�
    [SerializeField, Header("ajustAngle�x���ɕ␳�|�C���g��u��")] float adjustAngle;
    Vector3[] point; // �J�����ʒu�̕␳ ��苗���܂ŋߕt�����炱�̍��W�Ɉړ�������

    // ��]
    [SerializeField, Header("�h���b�O�̊��x")] float sensitivity;
    Vector2 sPos;    // �^�b�v�̎n�_
    Vector2 lastPos;  // �O�t���[���̃^�b�v�ʒu
    float wid, hei;  // �X�N���[���T�C�Y
    float tx, ty;

    // �g�k
    float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] float vSpeed;

    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    float dragAdjust = 10;

    [SerializeField, Header("��]�E�k�����ł���͈� �ŏ�")] Vector2 dragRangeMin;
    [SerializeField, Header("��]�E�k�����ł���͈� �ő�")] Vector2 dragRangeMax;

    Vector2[] dragRangeVertex = new Vector2[5]; // �^�b�v�\�͈͂̒��S��4���_

    // �_�u���^�b�v
    int tapCount = 0;
    bool canJudgement = false;
    float doubleTapTime = 0;
    bool is90Rotate = false;

    DoubleTap_TeleportDir inputDir1 = DoubleTap_TeleportDir.NULL; // ���ڂ̗���
    DoubleTap_TeleportDir inputDir2 = DoubleTap_TeleportDir.NULL; // ���ڂ̗���

    // �ړ�����
    enum DoubleTap_TeleportDir { UP, DOWN, RIGHT, LEFT, NULL };
    DoubleTap_TeleportDir teleportDir = DoubleTap_TeleportDir.NULL;

    // 90�x���̕␳�ʒu ��ʒ[���_�u���^�b�v�ł����̈ʒu�Ɉړ�����
    Vector3[] adjustPoint_90 = new Vector3[5];

    // �^�b�v/�X���C�v�\�͈͂�`��
    [SerializeField] Texture _texture;
    [SerializeField] bool isDragRangeDraw = false;

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
    }

    void Update()
    {
        if (!CanRotate) return;

        DoubleTapRotate();

        // ��]
        if (Input.touchCount == 1)
        {
            Touch t1 = Input.GetTouch(0);

            if (t1.phase == TouchPhase.Began)
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax)) return;

                sPos = t1.position;
                lastPos = t1.position;
            }
            else if (t1.phase == TouchPhase.Moved)
            {
                // �͈͊O����X���C�v�����Ƃ��p�̒���
                if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax))
                {
                    sPos = t1.position;
                    lastPos = t1.position;
                }

                // �X���C�v��
                tx = t1.position.x - lastPos.x;
                ty = t1.position.y - lastPos.y;

                // �ړ��ʂ����]�p�x�����߂�
                // �n�_����̈ړ��ʂ̐�Βl���AdragAjust��菬����������0�ɂ��A����/�����̈ړ��ɂ���
                if (Mathf.Abs(sPos.x - t1.position.x) < dragAdjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(sPos.y - t1.position.y) < dragAdjust) ty = 0;
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
                if (pos.y <= 0.005f)
                {
                    CameraPosAdjust();
                }

                lastPos = t1.position;
            }
        }

        // �g�k
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            // �͈͊O�͖���
            if (!stageController.TapOrDragRange(t1.position, dragRangeMin, dragRangeMax)) return;
            if (!stageController.TapOrDragRange(t2.position, dragRangeMin, dragRangeMax)) return;

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
        // 1��ڂ̃^�b�v
        if (Input.GetMouseButtonDown(0) && tapCount == 0)
        {
            // �͈͊O�͖���
            if (!stageController.TapOrDragRange(Input.mousePosition)) return;
            if (stageController.TapOrDragRange(Input.mousePosition, dragRangeMin, dragRangeMax)) return;

            // ���͕�����ۑ�
            inputDir1 = DoubleTapPosJudgement(Input.mousePosition);

            tapCount++;
            canJudgement = true;
        }

        if (canJudgement) doubleTapTime += Time.deltaTime;

        // 2��ڂ̃^�b�v
        if (doubleTapTime <= 0.2f && doubleTapTime >= 0.05f && tapCount == 1)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // �͈͊O�͖���
                if (!stageController.TapOrDragRange(Input.mousePosition)) return;
                if (stageController.TapOrDragRange(Input.mousePosition, dragRangeMin, dragRangeMax)) return;

                // ���͕�����ۑ�
                inputDir2 = DoubleTapPosJudgement(Input.mousePosition);

                tapCount++;
            }
        }
        else if (doubleTapTime > 0.2f)
        {
            canJudgement = false;
            tapCount = 0;
            doubleTapTime = 0f;
            inputDir1 = DoubleTap_TeleportDir.NULL;
            inputDir2 = DoubleTap_TeleportDir.NULL;
        }

        if (tapCount >= 2)
        {
            if(inputDir1 == inputDir2 && inputDir1 != DoubleTap_TeleportDir.NULL && inputDir2 != DoubleTap_TeleportDir.NULL)
                teleportDir = inputDir1;

            canJudgement = false;
            doubleTapTime = 0f;
            is90Rotate = true;
        }

        if(is90Rotate)
        {
            Camera90Rotate();
            is90Rotate = false;
            tapCount = 0;
            inputDir1 = DoubleTap_TeleportDir.NULL;
            inputDir2 = DoubleTap_TeleportDir.NULL;
        }
    }

    void Camera90Rotate()
    {
        Vector3 crossMin_right = new Vector3(0, 0, -100);
        Vector3 crossMin_left = new Vector3(0, 0, 100);
        Vector3 crossMin_up = new Vector3(100, 0, 0);
        Vector3 crossMin_down = new Vector3(-100, 0, 0);

        Vector3 near_right = Vector3.zero;
        Vector3 near_left = Vector3.zero;
        Vector3 near_up = Vector3.zero;
        Vector3 near_down = Vector3.zero;

        for (int i = 0; i < adjustPoint_90.Length; i++)
        {
            var f = transform.forward;
            var v = adjustPoint_90[i] - transform.position;
            var cross = Vector3.Cross(f, v);

            Debug.Log(i + " : " + cross);

            // ���E����
            if(cross.z < 0 && Mathf.Abs(cross.z) < Mathf.Abs(crossMin_right.z))
            {
                crossMin_right = cross;
                near_right = adjustPoint_90[i];
            }
            else if (cross.z > 0 && Mathf.Abs(cross.z) < Mathf.Abs(crossMin_left.z))
            {
                crossMin_left = cross;
                near_left = adjustPoint_90[i];
            }

            // �㉺����
            if (cross.x > 0 && Mathf.Abs(cross.x) < Mathf.Abs(crossMin_up.x))
            {
                crossMin_up = cross;
                near_up = adjustPoint_90[i];
            }
            else if (cross.x < 0 && Mathf.Abs(cross.x) < Mathf.Abs(crossMin_down.x))
            {
                crossMin_down = cross;
                near_down = adjustPoint_90[i];
            }
        }

        Vector3 cPos = transform.position;
        switch(teleportDir)
        {
            case DoubleTap_TeleportDir.UP:
                cPos = near_up;
                break;
            case DoubleTap_TeleportDir.DOWN:
                cPos = near_down;
                break;
            case DoubleTap_TeleportDir.RIGHT:
                cPos = near_right;
                break;
            case DoubleTap_TeleportDir.LEFT:
                cPos = near_left;
                break;
            default:
                cPos = transform.position;
                break;
        }

        transform.position = cPos;

        if (cPos.y > 0) transform.LookAt(target, Vector3.back);
        else transform.LookAt(target, Vector3.up);
    }

    /// <summary>
    /// �^�b�v�ʒu���㉺���E�͈̔͂̂ǂ��ɂ��邩�𔻒肵�A�ړ�������Ԃ�
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns>������Ԃ�</returns>
    DoubleTap_TeleportDir DoubleTapPosJudgement(Vector2 _pos)
    {
        // Input.mouseposition�͌��_���Ⴄ�̂Œ���
        _pos.y *= -1;
        _pos.y += hei;

        // �㕔��
        if (InsideOrOutsideJudgement(dragRangeVertex[2], dragRangeVertex[1], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.UP;

        // ������
        if (InsideOrOutsideJudgement(dragRangeVertex[3], dragRangeVertex[4], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.DOWN;

        // �E����
        if (InsideOrOutsideJudgement(dragRangeVertex[4], dragRangeVertex[2], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.RIGHT;

        // ������
        if (InsideOrOutsideJudgement(dragRangeVertex[1], dragRangeVertex[3], dragRangeVertex[0], _pos))
            return DoubleTap_TeleportDir.LEFT;

        return DoubleTap_TeleportDir.NULL;
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
        transform.position = defaultPos;
        transform.rotation = defaultRot;

        _camera.fieldOfView = vDefault;

        transform.LookAt(target, transform.up);
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

        // �T���v���̃T�C�Y�ɉ����ăJ�����ʒu�𒲐�
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 10, mapSize.z + 2);
        //_camera.orthographicSize = mapSize.x + 2;
        defaultPos = transform.position;
        defaultRot = transform.rotation;

        // �����ʒu�ݒ�
        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;
        target += new Vector3(0.5f, 0, -0.5f);
        
        transform.LookAt(target, transform.up);

        // �␳�|�C���g�̐���
        float r = (target - defaultPos).magnitude;
        Vector3 firstPos = new Vector3(r, 0, 0);
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }

        // 90�x���̕␳�|�C���g�̍쐬
        adjustPoint_90[0] = new Vector3(target.x, r, target.z);
        for (int i = 1; i < adjustPoint_90.Length; i++)
        {
            adjustPoint_90[i] = Quaternion.Euler(0, i * 90, 0) * firstPos + target;
        }

        //! Todo �T�C�Y�ɉ����āAField of View�̏����l�E�ő�l�E�ŏ��l���ύX
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
