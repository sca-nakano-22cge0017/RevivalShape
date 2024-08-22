using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// �I�u�W�F�N�g�P�̂̃A�E�g���C���̕`�搧��
/// </summary>
public class OutlineManager : MonoBehaviour
{
    private Transform fieldParent;
    private Camera camera;

    public void SetParent(Transform _parent)
    {
        fieldParent = _parent;
    }

    private void Awake()
    {
        
    }

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    void CheckOutlineObject()
    {
        Outline[] outlineObjects = fieldParent.GetComponentsInChildren<Outline>();
        List<Outline> outlineObjectsList = new List<Outline>();
        for (int i = 1; i < outlineObjects.Length; i++)
        {
            outlineObjectsList.Add(outlineObjects[i]);
        }


    }

    /// <summary>
    /// �J�������牓�����ɕ��בւ���
    /// </summary>
    List<Outline> ObjectsSort(List<Outline> objs)
    {
        List<Outline> result = objs;

        var dict = new Dictionary<float, Outline>();

        for (int i = 0; i < objs.Count; i++)
        {
            var dist = (objs[i].gameObject.transform.position - camera.transform.position).magnitude;
            dict.Add(dist, objs[i]);        }

        return result;
    }
}
