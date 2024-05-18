using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �m�F�t�F�[�Y�@�J������]
/// </summary>
public class CameraRotate : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] Camera _camera;
    Vector3 mapSize;

    Vector3 target;

    // ��]
    [SerializeField, Header("�h���b�O�̊��x")] float sensitivity;
    Vector2 sPos;   //�^�b�`�������W
    float wid, hei;  //�X�N���[���T�C�Y
    float tx, ty;

    Vector3 defaultPos;
    Quaternion defaultRot;

    // �g�k
    float sDist = 0.0f, nDist = 0.0f; //�����ϐ�
    float vRatio = 1.0f; //���ݔ{��
    [SerializeField, Header("Field of View�̍ő�E�Œ�l")] float vMax, vMin;
    [SerializeField, Header("Field of View�̏����l")] float vDefault;
    [SerializeField, Header("��{�̊g�k���x")] float vSpeed;

    [SerializeField, Header("�X���C�v�͈̔� �ŏ�")] Vector2 dragRangeMin;
    [SerializeField, Header("�X���C�v�͈̔� �ő�")] Vector2 dragRangeMax;

    bool canRotate = false;
    public bool CanRotate { get { return canRotate; }  set { canRotate = value; } }

    Vector3 movedPos = new Vector3(0, 0, 0); // debug

    void Start()
    {
        wid = Screen.width;
        hei = Screen.height;
    }

    //! ��]�̃��O�Ȃ���
    void Update()
    {
        if(!canRotate) return;

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
                tx = (t1.position.x - sPos.x) / wid; //���ړ���(-1<tx<1)
                ty = (t1.position.y - sPos.y) / hei; //�c�ړ���(-1<ty<1)

                // �ړ��ʂ��狁�߂���]�p�x
                float deltaAngleLR = tx * sensitivity;
                float deltaAngleTB = -ty * sensitivity;

                var angleAxisLR = Quaternion.AngleAxis(deltaAngleLR, transform.up);
                var angleAxisTB = Quaternion.AngleAxis(deltaAngleTB, transform.right);

                var pos = transform.position;
                pos -= target;
                pos = angleAxisLR * angleAxisTB * pos;
                pos += target;

                if(pos.y > 0)
                {
                    transform.position = pos;
                    transform.rotation = angleAxisLR * angleAxisTB * transform.rotation;
                }
            }
        }
        else if(Input.touchCount == 2)
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
                sDist = Vector2.Distance(t1.position, t2.position);

                vRatio = _camera.fieldOfView;
            }
            else if (t1.phase == TouchPhase.Moved || t2.phase == TouchPhase.Moved)
            {
                nDist = Vector2.Distance(t1.position, t2.position);
                if (sDist > nDist) vRatio += vSpeed * (1 - (vRatio - vMin) / (vMax - vMin)) * (sDist - nDist); // �k��
                if (sDist < nDist) vRatio -= vSpeed * ((vRatio - vMin) / (vMax - vMin)) * (nDist - sDist); // �g��
                _camera.fieldOfView = vRatio;
                sDist = nDist;
            }
        }

#if UNITY_EDITOR
        if(Input.GetMouseButtonDown(0))
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
        mapSize = stageController.MapSize; // �T�C�Y���

        target = mapSize / 2;
        target.x *= -1;
        target.y = 0;

        // �T���v���̃T�C�Y�ɉ����ăJ�����ʒu�𒲐�
        transform.position = new Vector3(-mapSize.x / 2 + 0.5f, mapSize.x + 10, mapSize.z + 2);
        defaultPos = transform.position;
        defaultRot = transform.rotation;

        //! �T�C�Y�ɉ����āAField of View�̏����l�E�ő�l�E�ŏ��l���ύX
    }
}
