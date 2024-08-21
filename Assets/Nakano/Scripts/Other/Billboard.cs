using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float scale;

    void Update()
    {
        var rot = Camera.main.transform.rotation;

        if(rot.x <= Quaternion.Euler(90, 0, 0).x) transform.rotation = Camera.main.transform.rotation;
        transform.localScale = Vector3.one * scale * ((float)Camera.main.fieldOfView / 179.0f);
    }
}
