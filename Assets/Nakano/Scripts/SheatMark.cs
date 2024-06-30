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
    private Vector3 markPos;
    [SerializeField] private GameObject markPoint;

    [SerializeField] private GameObject anotherMarkPoint;

    [SerializeField] private float scale;

    void Update()
    {
        markPos = markPoint.transform.position;
        GetComponent<RectTransform>().position = markPos;

        transform.localScale = Vector3.one * scale * GetDistance();

        float disFromCamera = (markPos - Camera.main.transform.position).magnitude;
        float disAnother = (anotherMarkPoint.transform.position - Camera.main.transform.position).magnitude;

        // ������̃}�[�N���J�����ɋ߂����
        if (disFromCamera < disAnother)
        {
            // ��ԏ�ɕ`�悷��
            this.transform.SetAsLastSibling();
        }
    }

    private float GetDistance()
    {
        return (transform.position - Camera.main.transform.position).magnitude;
    }
}
