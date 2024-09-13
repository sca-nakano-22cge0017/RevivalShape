using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ChangeRendering : MonoBehaviour
{
    [SerializeField] private Camera m_camera;
    [Header("0：アウトラインあり　1：アウトラインなし")] public int rendererMode;
    [SerializeField] private Toggle outlineSetting;

    void Start()
    {
        Change(rendererMode);
    }

    public void Change(int _rendererMode)
    {
        var cameraData = GetComponent<Camera>().GetComponent<UniversalAdditionalCameraData>();
        cameraData.SetRenderer(_rendererMode);
    }

    public void Change()
    {
        int _rendererMode = outlineSetting.isOn ? 0 : 1;

        var cameraData = GetComponent<Camera>().GetComponent<UniversalAdditionalCameraData>();
        cameraData.SetRenderer(_rendererMode);
    }
}
