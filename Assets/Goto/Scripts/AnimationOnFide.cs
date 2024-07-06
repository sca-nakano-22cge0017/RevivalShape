using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnFide : MonoBehaviour
{

    //===== ’è‹`—Ìˆæ =====
    public Animator anim;  
    //===== ‰Šúˆ— =====
    void Start()
    {
        PlayAnim();
      
    }

    public void PlayAnim()
    {
        anim.Play("ChenzPlay");
    }
    public void PlayAnim1()
    {
        anim.Play("ChenzSelect");
    }
}

