using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScript : MonoBehaviour
{

    [SerializeField]
    Material FadeMaterial;

    void Start()
    {
        StartCoroutine(FadeRoutine());
    }

    IEnumerator FadeRoutine()
    {

        // 黒＋全透明に初期化
        Color bl = Color.black;
        bl.a = 0;
        FadeMaterial.color = bl;

        // 黒くフェードさせるループ
        while (FadeMaterial.color.a < 0.8f)
        {
            yield return null;
            Color col = FadeMaterial.color;
            col.a += 0.02f;
            FadeMaterial.color = col;
        }
    }
}