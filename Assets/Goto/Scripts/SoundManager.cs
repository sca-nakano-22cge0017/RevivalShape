using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] BGM = null;
    [SerializeField] private AudioClip[] SE  =null;

    [SerializeField] private AudioSource audioSource;
   
    public void BGMPlay()
    {
        audioSource.clip = BGM[0];
        audioSource.Play();
    }
    public void BGMPlay1()
    {
        audioSource.clip = BGM[1];
        audioSource.Play();
    }
    public void BGMPlay2()
    {
        audioSource.clip = BGM[2];
        audioSource.Play();
    }
    public void SEPlay()
    {
        audioSource.PlayOneShot(SE[0]);
    }
    public void SEPlay1()
    {
        audioSource.PlayOneShot(SE[1]);
    }
    public void SEPlay2()
    {
        audioSource.PlayOneShot(SE[2]);
    }
    public void SEPlay3()
    {
        audioSource.PlayOneShot(SE[3]);
    }
    public void SEPlay4()
    {
        audioSource.PlayOneShot(SE[4]);
    }
    public void SEPlay5()
    {
        audioSource.PlayOneShot(SE[5]);
    }
    public void SEPlay6()
    {
        audioSource.PlayOneShot(SE[6]);
    }
    public void ClearSEPlay()
    {
        audioSource.PlayOneShot(SE[7]);
    }
    public void ClearSEPlay1()
    {
        audioSource.PlayOneShot(SE[8]);
    }

}
