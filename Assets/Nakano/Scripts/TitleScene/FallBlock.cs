using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallBlock : MonoBehaviour
{
    [HideInInspector] public BlockFallDirection blockFallDirection;
    [HideInInspector] public float targetHeight;
    [HideInInspector] public float fallSpeed;
    [HideInInspector] public bool isFall = false;

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
            }
            else
            {
                var pos = transform.localPosition;
                pos.y = targetHeight;
                transform.localPosition = pos;
                isFall = false;
            }
        }
    }
}
