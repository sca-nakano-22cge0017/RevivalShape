using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class TextTest : MonoBehaviour
{
    [SerializeField]
    private Text timerText;
    private float seconds;
    private float nowTime;
    private int minute;
    private float startTime;

    private bool timeActive = true;
    // Start is called before the first frame update
    void Start()
    {
        nowTime = 0f;
        seconds = 0f;
        timerText = GetComponentInChildren<Text>();
    }

    // Update is called once per frame
    void Update()
    {
        if (timeActive)
        {
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
    }

    public void OnStart()
    {
        timeActive = true;
    }
    public void OnStop()
    {
        timeActive = false;
        Debug.Log("Ž~‚ß‚é");
    }
}
