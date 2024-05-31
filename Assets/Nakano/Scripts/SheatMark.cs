using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �V�[�g��̑O��̃}�[�N�̐���
/// ��ɃJ��������������
/// </summary>
public class SheatMark : MonoBehaviour
{
    Vector3 markPos;
    [SerializeField] GameObject markPoint;

    [SerializeField] GameObject anotherMarkPoint;

    void Start()
    {
    }

    void Update()
    {
        markPos = markPoint.transform.position;
        GetComponent<RectTransform>().position = markPos;

        float disFromCamera = (markPos - Camera.main.transform.position).magnitude;
        float disAnother = (anotherMarkPoint.transform.position - Camera.main.transform.position).magnitude;

        // ������̃}�[�N���J�����ɋ߂����
        if(disFromCamera < disAnother)
        {
            // ��ԏ�ɕ`�悷��
            this.transform.SetAsLastSibling();
        }
    }
}
