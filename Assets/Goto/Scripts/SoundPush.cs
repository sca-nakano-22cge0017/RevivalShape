using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoundPush : MonoBehaviour
{
   
    private AudioSource audioSource;


    // Start is called before the first frame update
    void Start()
    {
    
        audioSource = GetComponent<AudioSource>();
        

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //public void OnPlaySound()
    //{
      //  audioSource.PlayOneShot(clip);
    //}
    public void SoundSliderOnValueChange(float newSliderValue)
    {
        // ���y�̉��ʂ��X���C�h�o�[�̒l�ɕύX
        audioSource.volume = newSliderValue;
    }
}
