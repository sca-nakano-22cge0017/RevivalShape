using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField]
    private Text testTime;
    private Text timerText;
    private int minute;
    private float seconds;
    private float oldSeconds;
    public float maxTime;
    //Å@ç≈èâÇÃéûä‘
    private float startTime;

    void Start()
    {
        timerText = GetComponentInChildren<Text>();
        oldSeconds = 0;
        startTime = Time.time;

        maxTime = 700;
    }

    void Update()
    {

        maxTime += Time.deltaTime;
        //Å@Time.timeÇ≈ÇÃéûä‘åvë™
        seconds = Time.time - startTime;

        minute = (int)seconds / 60;

       

        if ((int)seconds != (int)oldSeconds)
        {
            timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
            testTime.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
        }

        oldSeconds = seconds;

    
        if (Input.GetMouseButtonDown(1))
        {
            Time.timeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : 0f;
        }
        //Debug.Log(Time.time);
    }
    public void TimeStop()
    {
        Time.timeScale = Mathf.Approximately(Time.timeScale, 0f) ? 1f : 0f;
        timerText.text = minute.ToString("00") + ":" + ((int)(seconds % 60)).ToString("00");
    }
}