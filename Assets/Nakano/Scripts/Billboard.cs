using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private float scale;

    void Start()
    {
        
    }

    void Update()
    {
        transform.localScale = Vector3.one * scale * GetDistance();
    }

    private float GetDistance()
    {
        return (transform.position - Camera.main.transform.position).magnitude;
    }
}
