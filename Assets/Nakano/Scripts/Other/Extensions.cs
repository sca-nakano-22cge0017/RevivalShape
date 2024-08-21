using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 自作機能
/// </summary>
public class Extensions : MonoBehaviour
{
    [Header("図形描画")]
    [SerializeField, Header("描画範囲 左上")] private Vector2 drawRangeMin = new Vector2(0, 370);
    [SerializeField, Header("描画範囲 右下")] private Vector2 drawRangeMax = new Vector2(1080, 1700);
    [SerializeField] private Texture _texture;
    [SerializeField] private bool isDragRangeDraw = false;

    private void Awake()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.red);
        texture.Apply();
        _texture = texture;
    }

    private void OnGUI()
    {
        if (isDragRangeDraw)
        {
            var rect = new Rect(drawRangeMin.x, drawRangeMin.y, drawRangeMax.x - drawRangeMin.x, drawRangeMax.y - drawRangeMin.y);
            GUI.DrawTexture(rect, _texture);
        }
    }

    /// <summary>
    /// _seconds分待機してから_actionを実行
    /// </summary>
    /// <param name="_seconds">待機時間</param>
    /// <param name="_action">処理</param>
    /// <returns></returns>
    public static IEnumerator DelayCoroutine(float _seconds, Action _action)
    {
        yield return new WaitForSeconds(_seconds);
        _action?.Invoke();
    }

    /// <summary>
    /// _flagの返り値がtrueになったら_actionを実行
    /// </summary>
    /// <param name="_flag">処理実行のフラグ</param>
    /// <param name="_action">処理</param>
    /// <returns></returns>
    public static IEnumerator DelayCoroutine(Func<bool> _flag, Action _action)
    {
        bool flag = false;

        while (!flag)
        {
            flag = _flag.Invoke();
            yield return null;
        }

        _action?.Invoke();
    }
}
