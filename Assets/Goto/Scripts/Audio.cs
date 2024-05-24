using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;


public class Audio : MonoBehaviour
{
  
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] SoundPush soundPush;

 
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;
    [SerializeField] Slider MasterSlider;
    

    private void Start()
    {
        audioMixer.GetFloat("BGM", out float bgmVolume);
        BGMSlider.value = bgmVolume;
        
        audioMixer.GetFloat("SE", out float seVolume);
        SESlider.value = seVolume;

        audioMixer.GetFloat("Master", out float masterVolume);
        MasterSlider.value = masterVolume;

        float masterMaxSound = 20f;
        float masternowSound = 5f;


        float maxSound = 20f;
        float minSound = -80f;
        float nowSound = 5f;

        MasterSlider.maxValue = masterMaxSound;
        MasterSlider.minValue = minSound;
        MasterSlider.value = masternowSound;

        BGMSlider.maxValue = maxSound;
        BGMSlider.minValue = minSound;
        BGMSlider.value = nowSound;

        SESlider.maxValue = maxSound;
        SESlider.minValue = minSound;
        SESlider.value = nowSound;

        //soundPush.SoundSliderOnValueChange();
    }
    public void SetBGM(float volume)
    {
        audioMixer.SetFloat("BGM", volume);
    }

    public void SetSE(float volume)
    {
        audioMixer.SetFloat("SE", volume);
    }
    public void SetMaster(float volume)
    {
        audioMixer.SetFloat("Master", volume);
    }
  
}