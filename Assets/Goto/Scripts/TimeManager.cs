using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    private Text timerText;
    private int minute;
    private float seconds;
    private float oldSeconds;
    //　最初の時間
    private float startTime;

    void Start()
    {
        timerText = GetComponentInChildren<Text>();
        oldSeconds = 0;
        startTime = Time.time;
    }

    void Update()
    {

        //　Time.timeでの時間計測
        seconds = Time.time - startTime;

        minute = (int)seconds / 60;

        if ((int)seconds != (int)oldSeconds)
        {
            timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
        }
        oldSeconds = seconds;

        //　マウスの左ボタン押しで一時停止
        if (Input.GetMouseButtonDown(0))
        {
            Time.timeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : 0f;
        }
    }
}