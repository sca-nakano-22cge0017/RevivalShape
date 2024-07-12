using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioClip[] BGM = null;
    [SerializeField] private AudioClip[] SE  =null;

    [SerializeField] private AudioSource audioSourceSE;
    [SerializeField] private AudioSource audioSourceBGM;

    // public bool DontDestroyEnabled = true;
    private void Awake()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("SaveSoundManager");

        if (objects.Length > 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    public void Start()
    {
        //if (DontDestroyEnabled)
        //{
        //    // Sceneを遷移してもオブジェクトが消えないようにする
        //    DontDestroyOnLoad(this);
        //}
        //audioSourceBGM.clip = BGM[1];
        // audioSourceBGM.Play();
    }
    public void BGMPlay1()
    {
        audioSourceBGM.clip = BGM[1];
        audioSourceBGM.Play();
    }
    public void BGMPlay2()
    {
        audioSourceBGM.clip = BGM[2];
        audioSourceBGM.Play();
    }
    public void BGMPlay3()
    {
        audioSourceBGM.clip = BGM[3];
        audioSourceBGM.Play();
    }
    public void SEPlay1()
    {
        audioSourceSE.PlayOneShot(SE[1]);
        audioSourceSE.clip = SE[1];
    }
    public void SEPlay2()
    {
        audioSourceSE.PlayOneShot(SE[2]);
        audioSourceSE.clip = SE[2];
    }
    public void SEPlay3()
    {
        audioSourceSE.PlayOneShot(SE[3]);
        audioSourceSE.clip = SE[3];
    }
    public void SEPlay4()
    {
        audioSourceSE.PlayOneShot(SE[4]);
        audioSourceSE.clip = SE[4];
    }
    public void SEPlay5()
    {
        audioSourceSE.PlayOneShot(SE[5]);
        audioSourceSE.clip = SE[5];
    }
    public void SEPlay6()
    {
        audioSourceSE.PlayOneShot(SE[6]);
        audioSourceSE.clip = SE[6];
    }
    public void SEPlay7()
    {
        audioSourceSE.PlayOneShot(SE[7]);
        audioSourceSE.clip = SE[7];
    }
    public void ClearSEPlay()
    {
        audioSourceSE.PlayOneShot(SE[8]);
        audioSourceSE.clip = SE[8];
    }
    public void ClearSEPlay1()
    {
        audioSourceSE.PlayOneShot(SE[9]);
        audioSourceSE.clip = SE[9];
    }

    public void BGMNone()
    {
        audioSourceBGM.clip = BGM[0];
    }
    public void SENone()
    {
        audioSourceSE.clip = SE[0];
    }


}
