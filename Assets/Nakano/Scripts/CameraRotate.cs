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

    [SerializeField, Header("ajustAngle�x���ɕ␳�|�C���g��u��")] float adjustAngle;
    Vector3[] point; // �J�����ʒu�̕␳ ��苗���܂ŋߕt�����炱�̍��W�Ɉړ�������

    // ��]
    [SerializeField, Header("�h���b�O�̊��x")] float sensitivity;
    Vector2 sPos;    //�^�b�`�������W
    float wid, hei;  //�X�N���[���T�C�Y
    float tx, ty;

    // �g�k
    float sDist = 0.0f, nDist = 0.0f;  //�����ϐ�
    float vRatio = 1.0f;               //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] float vSpeed;

    [SerializeField, Header("�X���C�v ��U��␳�l"), Tooltip("�΂ߕ����ւ̃X���C�v��x��/y���ɐ^�������Ȉړ��ɕ␳����")]
    float dragAjust = 10;

    [SerializeField, Header("�X���C�v�͈̔� �ŏ�")] Vector2 dragRangeMin;
    [SerializeField, Header("�X���C�v�͈̔� �ő�")] Vector2 dragRangeMax;

    /// <summary>
    /// �J�����𓮂������ǂ���
    /// false�̂Ƃ��͓����Ȃ�
    /// </summary>
    public bool CanRotate { get; set; } = false;

    Vector3 movedPos = new Vector3(0, 0, 0); // debug

    void Awake()
    {
        wid = Screen.width;
        hei = Screen.height;

        point = new Vector3[(int)(360 / adjustAngle)];
    }

    void Update()
    {
        if (!CanRotate) return;

        // ��]
        if (Input.touchCount == 1)
        {
            Touch t1 = Input.GetTouch(0);

            if (t1.position.x <= dragRangeMin.x || t1.position.x > dragRangeMax.x ||
                t1.position.y <= dragRangeMin.y || t1.position.y > dragRangeMax.y)
                return; // �͈͊O�Ȃ�I��

            if (t1.phase == TouchPhase.Began)
            {
                sPos = t1.position;
            }
            else if (t1.phase == TouchPhase.Moved)
            {
                // �X���C�v��
                tx = t1.position.x - sPos.x;
                ty = t1.position.y - sPos.y;

                // �ړ��ʂ����]�p�x�����߂�
                // dragAjust���ړ��ʂ̐�Βl��������������0�ɂ��A����/�����̈ړ��ɂ���
                if (Mathf.Abs(tx) < dragAjust) tx = 0;
                float deltaAngleLR = tx / wid * sensitivity;

                if (Mathf.Abs(ty) < dragAjust) ty = 0;
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

                sPos = t1.position;
            }
        }

        // �g�k
        else if (Input.touchCount == 2)
        {
            Touch t1 = Input.GetTouch(0);
            Touch t2 = Input.GetTouch(1);

            if (t1.position.x <= dragRangeMin.x || t1.position.x > dragRangeMax.x ||
                t1.position.y <= dragRangeMin.y || t1.position.y > dragRangeMax.y)
                return; // �͈͊O�Ȃ�I��

            if (t2.position.x <= dragRangeMin.x || t2.position.x > dragRangeMax.x ||
                t2.position.y <= dragRangeMin.y || t2.position.y > dragRangeMax.y)
                return; // �͈͊O�Ȃ�I��

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

#if false // PC��œ������ꍇ
        if (Input.GetMouseButtonDown(0))
        {
            movedPos = Input.mousePosition;
        }
        if (Input.GetMouseButton(0))
        {
            tx = (movedPos.x - Input.mousePosition.x) / wid;
            ty = (movedPos.y - Input.mousePosition.y) / hei;

            float deltaAngleLR = -tx * 5;
            float deltaAngleTB = ty * 5;

            var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up);
            var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right);

            var pos = transform.position;
            pos -= target;
            pos = angleAxisLR * angleAxisTB * pos;
            pos += target;

            if (pos.y > 0)
            {
                transform.position = pos;
                transform.rotation = angleAxisLR * angleAxisTB * transform.rotation;
            }
        }

        if (Input.GetKeyDown(KeyCode.I) || Input.GetKeyDown(KeyCode.O))
        {
            vRatio = _camera.fieldOfView;
        }

        if (Input.GetKey(KeyCode.I)) // �g��
        {
            vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin));
            _camera.fieldOfView = vRatio;
        }
        if (Input.GetKey(KeyCode.O)) // �k��
        {
            vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin));
            _camera.fieldOfView = vRatio;
        }
#endif
    }

    /// <summary>
    /// �J�����ʒu��␳����
    /// </summary>
    void CameraPosAdjust()
    {
        Vector3 nearPos = new Vector3(100, 100, 100);

        var pos = transform.position;
        
        foreach(var p in point)
        {
            var nearDiff = (pos - nearPos).magnitude;
            var diff = (pos - p).magnitude;

            if(nearDiff > diff) nearPos = p;
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

        // �T���v���̃T�C�Y�ɉ����ăJ�����ʒu�𒲐�
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 10, mapSize.z + 2);
        defaultPos = transform.position;
        defaultRot = transform.rotation;

        //! Todo �T�C�Y�ɉ����āAField of View�̏����l�E�ő�l�E�ŏ��l���ύX

        // �␳�|�C���g�̐���
        float r = (target - defaultPos).magnitude;
        Vector3 firstPos = new Vector3(r, 0, 0);
        
        for (int i = 0; i < point.Length; i++)
        {
            point[i] = Quaternion.Euler(0, i * adjustAngle, 0) * firstPos + target;
        }
    }
}
