using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ����@�\
/// </summary>
public class Extensions : MonoBehaviour
{
    [Header("�}�`�`��")]
    [SerializeField, Header("�`��͈� ����")] private Vector2 drawRangeMin = new Vector2(0, 370);
    [SerializeField, Header("�`��͈� �E��")] private Vector2 drawRangeMax = new Vector2(1080, 1700);
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
    /// _seconds���ҋ@���Ă���_action�����s
    /// </summary>
    /// <param name="_seconds">�ҋ@����</param>
    /// <param name="_action">����</param>
    /// <returns></returns>
    public static IEnumerator DelayCoroutine(float _seconds, Action _action)
    {
        yield return new WaitForSeconds(_seconds);
        _action?.Invoke();
    }
}
