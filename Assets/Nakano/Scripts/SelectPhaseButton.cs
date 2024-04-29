using UnityEngine;
using UnityEngine.UI;

public class SelectPhaseButton : MonoBehaviour
{
    int inputNum = 0;

    /// <summary>
    /// 各ボタンに入力された数
    /// </summary>
    public int InputNum
    {
        get { return inputNum; }
        set { inputNum = value; }
    }

    Text thisText;

    const int input_max = 10; // 入力できる最大値

    private void Start()
    {
        // ボタンの子オブジェクトのTextを取得
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    /// <summary>
    /// ボタンを押したらカウントを増やす
    /// </summary>
    public void CountUp()
    {
        inputNum++;

        if(inputNum > input_max) inputNum = 0;

        thisText.text = inputNum.ToString(); // 表示変更
    }
}
