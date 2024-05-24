using UnityEngine.Audio;
using UnityEngine;
using UnityEngine.UI;


public class Audio : MonoBehaviour
{
  
    [SerializeField] AudioMixer audioMixer;
    private SoundPush soundPush;

 
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;
    [SerializeField] Slider MasterSlider;


    private void Start()
    {
        InitializeSliders();
        AttachSliderListeners();
    }

    private void InitializeSliders()
    {
        float maxSound = 20f;
        float minSound = -80f;
        float nowSound =-10;
        float MasterSound =0;

        BGMSlider.maxValue = maxSound;
        BGMSlider.minValue = minSound;
        BGMSlider.value =nowSound;

        SESlider.maxValue = maxSound;
        SESlider.minValue = minSound;
        SESlider.value =nowSound;

        MasterSlider.maxValue = maxSound;
        MasterSlider.minValue = minSound;
        MasterSlider.value =MasterSound;

        if (audioMixer != null)
        {
            if (audioMixer.GetFloat("BGM", out float bgmVolume))
                BGMSlider.value = Mathf.Clamp(bgmVolume, minSound, maxSound);

            if (audioMixer.GetFloat("SE", out float seVolume))
                SESlider.value = Mathf.Clamp(seVolume, minSound, maxSound);

            if (audioMixer.GetFloat("Master", out float masterVolume))
                MasterSlider.value = Mathf.Clamp(masterVolume, minSound, maxSound);
        }
    }

    private void AttachSliderListeners()
    {
        BGMSlider.onValueChanged.AddListener(SetBGM);
        SESlider.onValueChanged.AddListener(SetSE);
        MasterSlider.onValueChanged.AddListener(SetMaster);
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