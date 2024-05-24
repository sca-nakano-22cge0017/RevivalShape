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

        float maxSound = 200f;
        float minSound = 0f;
        float nowSound = 100f;

        vSlider.minValue = minSound;
        vSlider.maxValue = maxSound;
        vSlider.value = nowSound;
       
      
        
    }
}
