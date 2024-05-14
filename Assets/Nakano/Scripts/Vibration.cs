using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vibration : MonoBehaviour
{
    /// <summary>
    /// í∑Ç¢êUìÆ
    /// </summary>
    /// <param name="cnt">êUìÆâÒêî</param>
    public void LongVibrate(int cnt)
    {
        StartCoroutine(Vibrate(cnt));
    }

    IEnumerator Vibrate(int cnt)
    {
        for (int i = 0; i < cnt; i++)
        {
            if (SystemInfo.supportsVibration)
            {
                Handheld.Vibrate();
            }
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

    public static void Vibrate(long milliseconds)
    {
        Debug.Log("vibrate");
        if (isAndroid())
            vibrator.Call("vibrate", milliseconds);
        else
            Handheld.Vibrate();
    }
    private static bool isAndroid()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
            return true;
#else
        return false;
#endif
    }
}
