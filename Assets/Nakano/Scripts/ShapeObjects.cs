using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeObjects : MonoBehaviour
{
    bool isVibrate = false;
    public bool IsVibrate { get; set; }

    private void OnCollisionEnter(Collision collision)
    {
        if(isVibrate)
            Vibration.Vibrate(3);
    }
}
