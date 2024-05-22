using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択フェーズで使用するボタン
/// </summary>
public class SelectPhaseButton : MonoBehaviour
{
    SelectPhase selectPhase;

    /// <summary>
    /// 各ボタンに入力された数
    /// </summary>
    public int InputNum { get; set; } = 0;

    Text thisText;

    // 入力できる最大値
    const int input_max = 10;

    // 長押し
    bool isCountForLongTap = false; // ロングタップ用のカウントを開始するフラグ
    bool isLongTap = false;         // 長押し中かどうか
    [SerializeField, Header("長押し中 数値が1上がるまでの時間")] float interval = 0.5f;
    [SerializeField, Header("長押し成立までの時間")] float longTapTime = 2.0f;
    float tapStartTime = 0;

    // 消去モード
    bool isEraserMode = false; // 消去モードかどうか

    // 確認モード
    bool isCheckMode = false; // 確認モードかどうか

    public bool IsCheck{ get; set; } = false; // 確認するマスのボタンか
    
    private void Start()
    {
        // ボタンの子オブジェクトのTextを取得
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();

        selectPhase = FindObjectOfType<SelectPhase>();
    }

    private void Update()
    {
        // 表示変更
        thisText.text = InputNum.ToString();

        isEraserMode = selectPhase.IsEraser;
        isCheckMode = selectPhase.IsCheck;

        // 長押しか確認開始
        if (isCountForLongTap)
        {
            // カウント開始
            tapStartTime += Time.deltaTime;

            // 一定時間経過したら長押しとして処理
            if(tapStartTime >= longTapTime)
            {
                isLongTap = true;
                isCountForLongTap = false;
                tapStartTime = 0;
            }
        }

        // 長押しなら
        if (isLongTap)
        {
            tapStartTime += Time.deltaTime;

            // 一定時間経過毎に増減処理
            if(interval <= tapStartTime)
            {
                tapStartTime = 0;

                if(!isEraserMode) InputNum++;
                else InputNum--;
            }
        }

        // 最低値/最高値を超えないようにする
        if (InputNum > input_max) InputNum = input_max;
        if (InputNum <= 0) InputNum = 0;

        if(!isCheckMode) IsCheck = false;
    }

    /// <summary>
    /// ボタンを押したらカウントを増やす
    /// </summary>
    public void CountUp()
    {
        // 確認モードのときに押されたらボタンに応じたマスに配置された図形を表示する
        if (isCheckMode)
        {
            IsCheck = true;
            selectPhase.CheckWindowDisp();
            return;
        }

        if (!isEraserMode) InputNum++;
        else InputNum--;

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
