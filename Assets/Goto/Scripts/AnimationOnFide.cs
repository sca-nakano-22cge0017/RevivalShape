using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnFide : MonoBehaviour
{

    //===== ��`�̈� =====
    public Animator anim;  
    //===== �������� =====
    void Start()
    {
        PlayAnim();
      
    }

    public void PlayAnim()
    {
        anim.SetBool("blRot", true);
    }
}

