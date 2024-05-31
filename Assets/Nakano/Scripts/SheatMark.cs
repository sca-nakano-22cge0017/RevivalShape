using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// シート上の前後のマークの制御
/// 常にカメラを見続ける
/// </summary>
public class SheatMark : MonoBehaviour
{
    Vector3 markPos;
    [SerializeField] GameObject markPoint;

    [SerializeField] GameObject anotherMarkPoint;

    void Start()
    {
    }

    void Update()
    {
        markPos = markPoint.transform.position;
        GetComponent<RectTransform>().position = markPos;

        float disFromCamera = (markPos - Camera.main.transform.position).magnitude;
        float disAnother = (anotherMarkPoint.transform.position - Camera.main.transform.position).magnitude;

        // もう一つのマークよりカメラに近ければ
        if(disFromCamera < disAnother)
        {
            // 一番上に描画する
            this.transform.SetAsLastSibling();
        }
    }
}
