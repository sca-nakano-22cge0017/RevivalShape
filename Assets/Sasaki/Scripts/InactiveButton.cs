using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InactiveButton : MonoBehaviour
{
    [SerializeField, Header("使えないボタン")] private GameObject[] buttonObj = null;
    [SerializeField, Header("Content")] private RectTransform content = null;
    [SerializeField, Header("エキストラボタンを引いた分のContentの縦幅")] private float contentHeight = 400.0f;
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
            //星が一定値にいったらエクストラボタンを表示
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
