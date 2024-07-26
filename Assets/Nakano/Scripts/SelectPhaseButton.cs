using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 選択フェーズで使用するボタン
/// </summary>
public class SelectPhaseButton : MonoBehaviour
{
    private StageController stageController;
    private Tutorial tutorial;

    [SerializeField] private Animator textMoveAnim;
    [SerializeField] private Text currentText;
    [SerializeField] private Text nextText;

    public SelectPhase selectPhase{ get; set;}

    private int inputNum = 0;
    /// <summary>
    /// 各ボタンに入力された数
    /// </summary>
    public int InputNum { get{ return inputNum; } set{ inputNum = value; } }

    private Vector2 position = new Vector2(0, 0);
    /// <summary>
    /// ボタンの位置
    /// </summary>
    public Vector2 Position { get { return position; } set { position = value;  } }

    private int max = 10;
    /// <summary>
    /// 入力できる最大値
    /// </summary>
    public int InputMax { get{ return max; } set { max = value; } }

    // 長押し
    private bool isCountForLongTap = false; // ロングタップ用のカウントを開始するフラグ
    private bool isLongTap = false;         // 長押し中かどうか
    [SerializeField, Header("長押し中 数値が1上がるまでの時間")] private float interval = 0.5f;
    [SerializeField, Header("長押し成立までの時間")] private float longTapTime = 2.0f;
    private float tapStartTime = 0;

    // 消去モード
    private bool isEraserMode = false; // 消去モードかどうか

    // 確認モード
    private bool isCheckMode = false; // 確認モードかどうか
    [SerializeField] private Image flame; // 確認カメラモード時の発光

    private bool isCheck = false;
    public bool IsCheck{ get { return isCheck; } set { isCheck = value; } } // 確認するマスのボタンか

    private bool isInAnimation = false; // アニメーション中か

    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        stageController = GameObject.FindObjectOfType<StageController>();
        tutorial = GameObject.FindObjectOfType<Tutorial>();

        var rt = GetComponent<RectTransform>();
        flame.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
        flame.enabled = false;
    }

    private void Update()
    {
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

                NumberChange();
            }
        }

        if (!isCheckMode)
        {
            IsCheck = false;
            flame.enabled = false;
        }
    }

    public void ShapeCheckEnd()
    {
        flame.enabled = false;
    }

    public void PointerDown()
    {
        CountUp();
        if(!isCheckMode) selectPhase.CanSwipInput = true;
    }

    public void PointerEnter()
    {
        if(selectPhase.CanSwipInput) CountUp();
    }

    public void PointerUp()
    {
        CountEnd();
        selectPhase.CanSwipInput = false;
    }

    public void PointerExit()
    {
        CountEnd();
    }

    /// <summary>
    /// ボタンを押したらカウントを増やす
    /// </summary>
    void CountUp()
    {
        if(stageController.IsTutorial)
        {
            tutorial.ToSelectC = true;
        }

        // 確認モードのときに押されたらボタンに応じたマスに配置された図形を表示する
        if (isCheckMode)
        {
            IsCheck = true;
            selectPhase.CheckWindowDisp(position);
            flame.enabled = true;
            return;
        }
        
        NumberChange();

        if(selectPhase.CanSwipInput) return;

        isCountForLongTap = true;
        tapStartTime = 0;
    }

    /// <summary>
    /// ボタンから手を離した時の処理
    /// </summary>
    void CountEnd()
    {
        isLongTap = false;
        isCountForLongTap = false;
        tapStartTime = 0;
    }

    void NumberChange()
    {
        // アニメーション中ならスキップ
        if(isInAnimation) return;

        if (!isEraserMode && inputNum < max)
        {
            StartCoroutine(CountAnimation(true));
        }
        else if (isEraserMode && inputNum > 0)
        {
            StartCoroutine(CountAnimation(false));
        }
    }

    /// <summary>
    /// 演出処理
    /// </summary>
    /// <param name="isCountUp">trueのとき増加　falseのとき減少</param>
    /// <returns></returns>
    IEnumerator CountAnimation(bool isCountUp)
    {
        isInAnimation = true;

        int add = isCountUp ? 1 : -1;
        string animBoolName = isCountUp ? "CountUp" : "CountDown";

        currentText.text = inputNum.ToString();
        nextText.text = (inputNum + add).ToString();

        // アニメーション中の場合は終わるまで待機
        yield return new WaitUntil(() => textMoveAnim.GetCurrentAnimatorStateInfo(0).IsName("Default"));
        textMoveAnim.SetTrigger(animBoolName);

        inputNum += add;
        selectPhase.ShapeInput(Position);  // 図形追加

        // アニメーション終了後、テキストを更新
        yield return new WaitUntil(() => textMoveAnim.GetCurrentAnimatorStateInfo(0).IsName("Default"));
        currentText.text = inputNum.ToString();

        isInAnimation = false;
    }
}
