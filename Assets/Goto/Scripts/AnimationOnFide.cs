using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationOnFide : MonoBehaviour
{

    //===== 定義領域 =====
    public Animator anim;  
    //===== 初期処理 =====
    void Start()
    {
        PlayAnim();
      
    }

    public void PlayAnim()
    {
        anim.SetBool("blRot", true);
    }
}

