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
  
    public float stopTime;
    //Å@ç≈èâÇÃéûä‘
    private float startTime;
    private  bool timeActive = true;
    private float nowTime;
    
    private float totalTime = 0;
    public float TotalTime { get { return totalTime; } private set{ totalTime = value; } }
    public bool TimeActive { get { return timeActive; } private set { timeActive = value; } }

    void Start()
    {
       
      
       // startTime = Time.time;
        nowTime = 0f;
        


    }

    void Update()
    {
        if (timeActive)
        {
            totalTime += Time.deltaTime;
            nowTime += Time.deltaTime;
            if (nowTime >= 60f)
            {
                minute++;
                nowTime = nowTime - 60;
            }
            // minute = (int)nowTime / 60;



            if ((int)nowTime != (int)seconds)
            {
                timerText.text = minute.ToString("00") + ":" + ((int)nowTime).ToString("00");
            }
            seconds = nowTime;
        }
        //Å@Time.timeÇ≈ÇÃéûä‘åvë™
        // seconds = Time.time - startTime;
        //minute = (int)seconds / 60;
        //if ((int)seconds != (int)oldSeconds)
        //{
        //    timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
        //}



        //oldSeconds = seconds;

        //Debug.Log(Time.time);
    }
    //public void TimeStop()
    //{
    //    Time.timeScale = 0;
    //    timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
    //}

    //public void TimeStart()
    //{
    //    Time.timeScale = 1;
    //    timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
    //}

    public void OnStart()
    {
        timeActive = true;
    }
    public void OnStop()
    {
        timeActive = false;
        Debug.Log("é~ÇﬂÇÈ");
    }
}