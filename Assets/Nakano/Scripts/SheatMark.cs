using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シート上の前後のマークの制御
/// 常にカメラを見続ける
/// </summary>
public class SheatMark : MonoBehaviour
{
    Vector3 cameraPos;

    void Start()
    {
    }

    void Update()
    {
        cameraPos = Camera.main.transform.position;

        transform.LookAt(cameraPos, -Camera.main.transform.up);
        var rot = Quaternion.AngleAxis(90f, transform.right);

        transform.rotation = rot * transform.rotation;

        var rotY = Quaternion.AngleAxis(180f, transform.up);
        transform.rotation = rotY * transform.rotation;
    }
}
