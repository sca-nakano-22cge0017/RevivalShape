using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField]
    private Text timerText;
    private int minute;
    private float seconds;
    private float oldSeconds;
    public float stopTime;
    //Å@ç≈èâÇÃéûä‘
    private float startTime;
    private  bool timeActive = true;

    void Start()
    {
        timerText = GetComponentInChildren<Text>();
        oldSeconds = 0;
        startTime = Time.time;

       
    }

    void Update()
    { 
        if (timeActive)
      {
            seconds = Time.time - startTime;
          
      }
        //Å@Time.timeÇ≈ÇÃéûä‘åvë™
       // seconds = Time.time - startTime;
        minute = (int)seconds / 60;
        if ((int)seconds != (int)oldSeconds)
        {
            timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
        }

        oldSeconds = seconds;

        //Debug.Log(Time.time);
    }
    public void TimeStop()
    {
        Time.timeScale = 0;
        timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
    }

    public void TimeStart()
    {
        Time.timeScale = 1;
        timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
    }

    public void OnStart()
    {
        timeActive = true;
    }
    public void OnStop()
    {
        timeActive = false;
    }
}