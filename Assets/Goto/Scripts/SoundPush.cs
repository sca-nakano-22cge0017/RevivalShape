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
    //    // 音楽の音量をスライドバーの値に変更
    //    audioSource.volume = Mathf.Clamp01(newSliderValue / 20f); // 0 から 1 の範囲に正規化
    //}
    public void SetVolume(float sliderValue)
    {
        // スライダーの値を0から1の範囲に正規化（20dBを1、-80dBを0とする）
        float normalizedVolume = Mathf.InverseLerp(-80f, 20f, sliderValue);
        audioSource.volume = Mathf.Clamp(normalizedVolume, 0f, 1f); // 0から1の範囲に収める
    }
}
