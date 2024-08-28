using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockFallDirection : MonoBehaviour
{
    [SerializeField] private float fallSpeed = 10;
    [SerializeField] private float coolTime;
    [SerializeField] FallBlock[] fallBlocks;
    [SerializeField] private float needTimeForUnClear;


    void Awake()
    {
        for(int i = 0; i < fallBlocks.Length; i++)
        {
            fallBlocks[i].blockFallDirection = this;
            fallBlocks[i].fallSpeed = fallSpeed;
            fallBlocks[i].needTimeForUnClear = needTimeForUnClear;
            fallBlocks[i].targetHeight = (i >= 0 && i < 3) || (i >= 6 && i < 9) ? 0 : 1;
        }

        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        WaitForSeconds cool = new WaitForSeconds(coolTime);

        foreach (var bf in fallBlocks)
        {
            bf.isFall = true;
            yield return cool;
        }
    }
}
