using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallBlock : MonoBehaviour
{
    [HideInInspector] public BlockFallDirection blockFallDirection;
    [HideInInspector] public float targetHeight;
    [HideInInspector] public float fallSpeed;
    [HideInInspector] public bool isFall = false;
    [HideInInspector] public float needTimeForUnClear;
    private MeshRenderer meshRenderer;
    private float alpha = 0;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        meshRenderer.material.color = new Color(1, 1, 1, alpha);
    }

    void Update()
    {
        if (isFall)
        {
            var tmpPos = this.transform;
            tmpPos.Translate(Vector3.down * fallSpeed  * Time.deltaTime);

            // —Ž‰ºˆ—
            if (this.transform.localPosition.y >= targetHeight)
            {
                transform.position = tmpPos.position;

                alpha += 1 / needTimeForUnClear * Time.deltaTime;
                meshRenderer.material.color = new Color(1, 1, 1, alpha);
            }
            else
            {
                var pos = transform.localPosition;
                pos.y = targetHeight;
                transform.localPosition = pos;
                isFall = false;

                alpha = 1;
                meshRenderer.material.color = new Color(1, 1, 1, alpha);
            }
        }

        if (alpha <= 0)
        {
            meshRenderer.enabled = false;
        }
        else meshRenderer.enabled = true;
    }
}
