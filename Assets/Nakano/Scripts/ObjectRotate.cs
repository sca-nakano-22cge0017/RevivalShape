using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �m�F�t�F�[�Y�@�I�u�W�F�N�g��]
/// </summary>
public class ObjectRotate : MonoBehaviour
{
    //��]�p
    Vector2 sPos;   //�^�b�`�������W
    Quaternion sRot;//�^�b�`�����Ƃ��̉�]
    float wid, hei, diag;  //�X�N���[���T�C�Y
    float tx, ty;    //�ϐ�

    void Start()
    {
        wid = Screen.width;
        hei = Screen.height;
        diag = Mathf.Sqrt(Mathf.Pow(wid, 2) + Mathf.Pow(hei, 2));
    }

    void Update()
    {
        if (Input.touchCount == 1)
        {
            //��]
            Touch t1 = Input.GetTouch(0);
            if (t1.phase == TouchPhase.Began)
            {
                sPos = t1.position;
                sRot = transform.rotation;
            }
            else if (t1.phase == TouchPhase.Moved || t1.phase == TouchPhase.Stationary)
            {
                tx = (t1.position.x - sPos.x) / wid; //���ړ���(-1<tx<1)
                ty = (t1.position.y - sPos.y) / hei; //�c�ړ���(-1<ty<1)
                transform.rotation = sRot;
                transform.Rotate(new Vector3(90 * ty, -90 * tx, 0), Space.World);
            }
        }
    }
}
