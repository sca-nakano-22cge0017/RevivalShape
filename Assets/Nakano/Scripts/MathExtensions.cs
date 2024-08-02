using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 数学系の自作拡張機能
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// 外積による内外判定
    /// </summary>
    /// <param name="_vertexLeft">底辺 左側の頂点</param>
    /// <param name="_vertexRight">底辺 右側の頂点</param>
    /// <param name="_vertexTop">上部の頂点</param>
    /// <param name="_judgePos">判定する位置</param>
    /// <returns>引数1～3を頂点とする三角形の内部にあればtrueを返す</returns>
    public static bool InsideOrOutsideJudgement(Vector2 _vertexLeft, Vector2 _vertexRight, Vector2 _vertexTop, Vector2 _judgePos)
    {
        // ベクトル
        Vector3 leftToTop = _vertexTop - _vertexLeft;
        Vector3 leftToRight = _vertexRight - _vertexLeft;
        Vector3 rightToTop = _vertexTop - _vertexRight;
        Vector3 leftToJudge = _judgePos - _vertexLeft;
        Vector3 rightToJudge = _judgePos - _vertexRight;

        // 外積計算
        Vector3 cross1 = Vector3.Cross(leftToJudge, leftToTop);
        Vector3 cross2 = Vector3.Cross(leftToRight, leftToJudge);
        Vector3 cross3 = Vector3.Cross(rightToTop, rightToJudge);

        // 外積のzの値の符号が同じなら三角形の内部に点が存在する
        if ((cross1.z > 0 && cross2.z > 0 && cross3.z > 0) || (cross1.z < 0 && cross2.z < 0 && cross3.z < 0))
            return true;

        else return false;
    }

    /// <summary>
    /// 投影したベクトル同士の角度を求める
    /// </summary>
    /// <param name="_from">投影するベクトル1</param>
    /// <param name="_to">投影するベクトル2</param>
    /// <param name="_normal">投影する面の法線ベクトル</param>
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
