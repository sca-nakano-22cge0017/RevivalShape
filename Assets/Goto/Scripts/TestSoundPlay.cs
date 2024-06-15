using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSoundPlay : MonoBehaviour
{
    public SoundManager soundManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void PushOn()
    {
        soundManager.SEPlay3();
    }
    void PushOn2()
    {
        soundManager.SEPlay4();
    }
}
