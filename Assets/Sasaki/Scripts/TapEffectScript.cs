using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapEffectScript : MonoBehaviour
{
    [SerializeField] private ParticleSystem tapEffect;
    [SerializeField] private Camera camera;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var pos = Input.mousePosition;
            pos.z = 10f;

            transform.position = camera.ScreenToWorldPoint(pos);
            tapEffect.Play();
        }
    }
}
