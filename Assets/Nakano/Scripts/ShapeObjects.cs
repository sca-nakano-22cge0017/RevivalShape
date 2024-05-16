using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeObjects : MonoBehaviour
{
    bool isVibrate = false;
    public bool IsVibrate { get{ return isVibrate; } set{ isVibrate = value; } }

    private void OnCollisionEnter(Collision collision)
    {
        if (isVibrate)
        {
            //Vibration.Vibrate(3);
            Handheld.Vibrate();
        }
    }
}
