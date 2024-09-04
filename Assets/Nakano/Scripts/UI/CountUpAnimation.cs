using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountUpAnimation : MonoBehaviour
{
    public bool isInAnimation = false;
    public bool isAnimationEnd = false;

    public void AnimationStart()
    {
        isInAnimation = true;
    }

    public void AnimationEnd()
    {
        isAnimationEnd = true;
    }
}
