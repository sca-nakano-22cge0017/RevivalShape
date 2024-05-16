using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Vibration : MonoBehaviour
{
    /// <summary>
    /// •¡”‰ñ‚ÌU“®
    /// </summary>
    /// <param name="cnt">U“®‰ñ”</param>
    /// <param name="milliseconds">U“®‚Ì’·‚³iƒ~ƒŠ•bj</param>
    public void PluralVibrate(int cnt, long milliseconds)
    {
        StartCoroutine(_PluralVibrate(cnt, milliseconds));
    }

    IEnumerator _PluralVibrate(int cnt, long milliseconds)
    {
        for (int i = 0; i < cnt; i++)
        {
            //Vibrate(milliseconds);
            Handheld.Vibrate();
            yield return new WaitForSeconds(0.6f);
        }
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
    public static AndroidJavaClass unityPlayer;
    public static AndroidJavaObject currentActivity;
    public static AndroidJavaObject vibrator;
#endif

    /// <summary>
    /// •b”w’è@U“®
    /// </summary>
    /// <param name="milliseconds">U“®‚Ì’·‚³(ƒ~ƒŠ•b)</param>
    public static void Vibrate(long milliseconds)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        Debug.Log("Vibrate");
        vibrator.Call("vibrate", milliseconds);
#endif
        if (milliseconds >= 1000)
        {
            Handheld.Vibrate();
        }
    }
}
