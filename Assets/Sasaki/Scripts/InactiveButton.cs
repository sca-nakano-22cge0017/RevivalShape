using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InactiveButton : MonoBehaviour
{
    [SerializeField, Header("�g���Ȃ��{�^��")] private GameObject[] buttonObj = null;
    [SerializeField, Header("Content")] private RectTransform content = null;
    [SerializeField, Header("�G�L�X�g���{�^��������������Content�̏c��")] private float contentHeight = 400.0f;
    private GameObject extraButton = null;

    float width = 0.0f;
    float height = 0.0f;
    void Start()
    {
        extraButton = GameObject.Find("Extra");

        width = content.sizeDelta.x;
        height = content.sizeDelta.y;
        for (int i = 0; i < buttonObj.Length - 2; i++)
        {
            buttonObj[i].GetComponent<Button>().interactable = true;
        }
    }
    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < buttonObj.Length - 2; i++)
        {
            //�������l�ɂ�������G�N�X�g���{�^����\��
            if (buttonObj[i].GetComponent<Button>().interactable == true)
            {
                extraButton.SetActive(true);
                content.sizeDelta = new Vector2(width, height);
            }
            else
            {
                extraButton.SetActive(false);
                content.sizeDelta = new Vector2(width, height - contentHeight);
            }
        }
    }
}
