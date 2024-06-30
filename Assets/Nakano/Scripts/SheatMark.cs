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
    private Vector3 markPos;
    [SerializeField] private GameObject markPoint;

    [SerializeField] private GameObject anotherMarkPoint;

    [SerializeField] private float scale;

    void Update()
    {
        markPos = markPoint.transform.position;
        GetComponent<RectTransform>().position = markPos;

        transform.localScale = Vector3.one * scale * GetDistance();

        float disFromCamera = (markPos - Camera.main.transform.position).magnitude;
        float disAnother = (anotherMarkPoint.transform.position - Camera.main.transform.position).magnitude;

        // もう一つのマークよりカメラに近ければ
        if (disFromCamera < disAnother)
        {
            // 一番上に描画する
            this.transform.SetAsLastSibling();
        }
    }

    private float GetDistance()
    {
        return (transform.position - Camera.main.transform.position).magnitude;
    }
}
