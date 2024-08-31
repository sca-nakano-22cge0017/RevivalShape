using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapEffectScript : MonoBehaviour
{
    [SerializeField] private ParticleSystem tapEffect;
    [SerializeField] private Camera mainCamera;

    private SoundManager sm;
    // Start is called before the first frame update
    void Start()
    {
        sm = FindObjectOfType<SoundManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if(sm != null) sm.SEPlay1();
            var pos = Input.mousePosition;
            pos.z = Vector3.Distance(new Vector3(0,0,tapEffect.transform.position.z), mainCamera.transform.position);

            tapEffect.transform.position = mainCamera.ScreenToWorldPoint(pos);
            tapEffect.Emit(8);
        }
    }
}
