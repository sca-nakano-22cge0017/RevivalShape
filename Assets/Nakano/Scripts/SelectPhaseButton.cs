using UnityEngine;
using UnityEngine.UI;

public class SelectPhaseButton : MonoBehaviour
{
    SelectPhase selectPhase;

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

    // 長押し
    bool isCountForLongTap = false; // ロングタップ用のカウントを開始するフラグ
    bool isLongTap = false; // 長押し中かどうか
    [SerializeField, Header("長押し中 数値が1上がるまでの時間")] float interval = 0.5f;
    [SerializeField, Header("長押し成立までの時間")] float longTapTime = 2.0f;
    float tapStartTime = 0;

    // 消去モード
    bool isEraserMode = false; // 消去モードかどうか

    // 確認モード
    bool isCheckMode = false; // 確認モードかどうか
    public bool isCheck{ get; set; } = false; // 確認するマスのボタンか
    
    private void Start()
    {
        // ボタンの子オブジェクトのTextを取得
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();

        selectPhase = FindObjectOfType<SelectPhase>();
    }

    private void Update()
    {
        thisText.text = inputNum.ToString(); // 表示変更

        isEraserMode = selectPhase.IsEraser;
        isCheckMode = selectPhase.IsCheck;

        // 長押し
        if (isCountForLongTap)
        {
            tapStartTime += Time.deltaTime;

            if(tapStartTime >= longTapTime)
            {
                isLongTap = true;
                isCountForLongTap = false;
                tapStartTime = 0;
            }
        }

        if (isLongTap)
        {
            tapStartTime += Time.deltaTime;
            if(interval <= tapStartTime)
            {
                tapStartTime = 0;

                if(!isEraserMode) inputNum++;
                else inputNum--;
            }
        }

        if (inputNum > input_max) inputNum = input_max;
        if (inputNum <= 0) inputNum = 0;

        if(!isCheckMode) isCheck = false;
    }

    /// <summary>
    /// ボタンを押したらカウントを増やす
    /// </summary>
    public void CountUp()
    {
        // 確認モードのときに押されたらボタンに応じたマスに配置された図形を表示する
        if (isCheckMode)
        {
            isCheck = true;
            selectPhase.CheckWindowDisp();
            return;
        }

        if (!isEraserMode) inputNum++;
        else inputNum--;

        isCountForLongTap = true;
        tapStartTime = 0;
    }

    /// <summary>
    /// ボタンから手を離した時の処理
    /// </summary>
    public void CountEnd()
    {
        isLongTap = false;
        isCountForLongTap = false;
        tapStartTime = 0;
    }
}
