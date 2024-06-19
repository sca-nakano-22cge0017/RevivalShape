using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResultText : MonoBehaviour
{
    [SerializeField,Header("�X�g�[���e�L�X�g")]private string[] texts = null;

    [SerializeField,Header("�\���Ԋu(�b)")]float textTime = 0.2f;

    int textNum;
    // Start is called before the first frame update
    void Start()
    {
        textNum = 0;
    }

    // Update is called once per frame
    void Update()
    {
        textMove();
    }

    void textMove()
    {
        textTime -= Time.deltaTime;

        if (textTime <= 0)
        {
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i] += "\n\n";
            }
        }
    }
}
