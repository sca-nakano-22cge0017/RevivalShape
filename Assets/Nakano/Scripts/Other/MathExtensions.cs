using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ���w�n�̎���g���@�\
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// �O�ςɂ����O����
    /// </summary>
    /// <param name="_vertexLeft">��� �����̒��_</param>
    /// <param name="_vertexRight">��� �E���̒��_</param>
    /// <param name="_vertexTop">�㕔�̒��_</param>
    /// <param name="_judgePos">���肷��ʒu</param>
    /// <returns>����1�`3�𒸓_�Ƃ���O�p�`�̓����ɂ����true��Ԃ�</returns>
    public static bool InsideOrOutsideJudgement(Vector2 _vertexLeft, Vector2 _vertexRight, Vector2 _vertexTop, Vector2 _judgePos)
    {
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
            return true;

        else return false;
    }

    /// <summary>
    /// ���e�����x�N�g�����m�̊p�x�����߂�
    /// </summary>
    /// <param name="_from">���e����x�N�g��1</param>
    /// <param name="_to">���e����x�N�g��2</param>
    /// <param name="_normal">���e����ʂ̖@���x�N�g��</param>
    /// <returns></returns>
    public static float ProjectionAngle(Vector3 _from, Vector3 _to, Vector3 _normal)
    {
        float angle = 0;

        var planeFrom = Vector3.ProjectOnPlane(_from, _normal);
        var planeTo = Vector3.ProjectOnPlane(_to, _normal);
        angle = Vector3.SignedAngle(planeFrom, planeTo, _normal);

        return angle;
    }
}
