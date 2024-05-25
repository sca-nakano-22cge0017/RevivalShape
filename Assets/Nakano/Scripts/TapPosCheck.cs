using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// タップ/ドラッグの位置が範囲外かどうかを判定する
/// </summary>
public class TapPosCheck : MonoBehaviour
{
    [SerializeField] Vector2 dragRangeMin;
    [SerializeField] Vector2 dragRangeMax;

    /// <summary>
    /// タップ/ドラッグできるかを判定する　範囲外ならfalse
    /// </summary>
    /// <param name="_pos">タップ位置</param>
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
    /// タップ/ドラッグできるかを判定する　範囲外ならfalse
    /// </summary>
    /// <param name="_pos">タップ位置</param>
    /// <param name="_minPos">最小値</param>
    /// <param name="_maxPos">最大値</param>
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
