using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundSetting : MonoBehaviour
{
    [SerializeField]
    Slider vSlider;

    // Start is called before the first frame update
    void Start()
    {
        vSlider = GetComponent<Slider>();

        float maxValue = 200f;
        float minValue = 0f;
        float nowValue = 100f;

        vSlider.minValue = minValue;
        vSlider.maxValue = maxValue;
        vSlider.value = nowValue;
      
        
    }
}
