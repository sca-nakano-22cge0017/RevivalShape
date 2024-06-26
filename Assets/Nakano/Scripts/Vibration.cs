using NativeUtil;
using System.Collections;
using UnityEngine;

public class Vibration : MonoBehaviour
{
    /// <summary>
    /// 複数回の振動
    /// </summary>
    /// <param name="cnt">振動回数</param>
    /// <param name="milliseconds">振動の長さ（ミリ秒）</param>
    public void PluralVibrate(int cnt, long milliseconds)
    {
        StartCoroutine(_PluralVibrate(cnt, milliseconds));
    }

    IEnumerator _PluralVibrate(int cnt, long milliseconds)
    {
        for (int i = 0; i < cnt; i++)
        {
            AndroidUtil.Vibrate(milliseconds);
            //Handheld.Vibrate();
            yield return new WaitForSeconds(0.6f);
        }
    }
}
