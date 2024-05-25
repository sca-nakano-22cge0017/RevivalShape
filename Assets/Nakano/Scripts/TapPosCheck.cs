using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �^�b�v/�h���b�O�̈ʒu���͈͊O���ǂ����𔻒肷��
/// </summary>
public class TapPosCheck : MonoBehaviour
{
    [SerializeField] Vector2 dragRangeMin;
    [SerializeField] Vector2 dragRangeMax;

    /// <summary>
    /// �^�b�v/�h���b�O�ł��邩�𔻒肷��@�͈͊O�Ȃ�false
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos)
    {
        bool canTap = false;

        var p = _pos;
        if (p.x <= dragRangeMin.x || p.x > dragRangeMax.x || p.y <= dragRangeMin.y || p.y > dragRangeMax.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }

    /// <summary>
    /// �^�b�v/�h���b�O�ł��邩�𔻒肷��@�͈͊O�Ȃ�false
    /// </summary>
    /// <param name="_pos">�^�b�v�ʒu</param>
    /// <param name="_minPos">�ŏ��l</param>
    /// <param name="_maxPos">�ő�l</param>
    /// <returns></returns>
    public bool TapOrDragRange(Vector3 _pos, Vector3 _minPos, Vector3 _maxPos)
    {
        bool canTap = false;

        var p = _pos;
        if (p.x <= _minPos.x || p.x > _maxPos.x || p.y <= _minPos.y || p.y > _maxPos.y)
            canTap = false;
        else canTap = true;

        return canTap;
    }
}
