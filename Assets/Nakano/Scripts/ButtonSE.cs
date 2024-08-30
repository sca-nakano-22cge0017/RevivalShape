using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonSE : MonoBehaviour
{
    // SE
    private SoundManager sm;

    private void Awake()
    {
        sm = FindObjectOfType<SoundManager>();
    }

    public void SEPlay()
    {
        if (sm != null)
        {
            sm.SEPlay2();
        }
    }
}
