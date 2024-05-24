using NativeUtil;
using System.Collections;
using UnityEngine;

public class Vibration : MonoBehaviour
{
    /// <summary>
    /// ������̐U��
    /// </summary>
    /// <param name="cnt">�U����</param>
    /// <param name="milliseconds">�U���̒����i�~���b�j</param>
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
