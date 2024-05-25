using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SensChenz : MonoBehaviour
{
    [SerializeField] Slider SensSlider;

    public float sensitivity { get; set; }
    // Start is called before the first frame update
    void Start()
    {
        float maxSens = 100f;
        float minSens = 0;
        float nowSens = 50;


        SensSlider.maxValue = maxSens;
        SensSlider.minValue = minSens;
        SensSlider.value = nowSens;
    

    }

    // Update is called once per frame
    void Update()
    {
        sensitivity = SensSlider.value;
        //Debug.Log(sensitivity);
    }
}
