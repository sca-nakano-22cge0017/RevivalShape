using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class SoundPush : MonoBehaviour
{
    [SerializeField]
    private AudioSource audioSource;

    //public void OnPlaySound()
    //{
    //  audioSource.PlayOneShot(clip);
    //}
    //public void SoundSliderOnValueChange(float newSliderValue)
    //{
    //    // ���y�̉��ʂ��X���C�h�o�[�̒l�ɕύX
    //    audioSource.volume = Mathf.Clamp01(newSliderValue / 20f); // 0 ���� 1 �͈̔͂ɐ��K��
    //}
    public void SetVolume(float sliderValue)
    {
        // �X���C�_�[�̒l��0����1�͈̔͂ɐ��K���i20dB��1�A-80dB��0�Ƃ���j
        float normalizedVolume = Mathf.InverseLerp(-80f, 20f, sliderValue);
        audioSource.volume = Mathf.Clamp(normalizedVolume, 0f, 1f); // 0����1�͈̔͂Ɏ��߂�
    }
}
