using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundSlider : MonoBehaviour
{
    [SerializeField] Slider BGMSlider;
    [SerializeField] Slider SESlider;
    [SerializeField] Slider MasterSlider;

    Audio audio;
    AudioMixer audioMixer;
    float maxSound = 20f;
    float minSound = -80f;
    float MasterSound = 0;

    void Start()
    {
        audio = FindObjectOfType<Audio>();
        audioMixer = audio.GetComponent<AudioMixer>();
        maxSound = audio.maxSound;
        minSound = audio.minSound;
        MasterSound = audio.MasterSound;

        InitializeSliders();
        AttachSliderListeners();
    }

    private void InitializeSliders()
    {
        BGMSlider.maxValue = maxSound;
        BGMSlider.minValue = minSound;

        SESlider.maxValue = maxSound;
        SESlider.minValue = minSound;

        MasterSlider.maxValue = maxSound;
        MasterSlider.minValue = minSound;
        MasterSlider.value = MasterSound;

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
        BGMSlider.onValueChanged.AddListener(audio.SetBGM);
        SESlider.onValueChanged.AddListener(audio.SetSE);
        MasterSlider.onValueChanged.AddListener(audio.SetMaster);
    }
}
