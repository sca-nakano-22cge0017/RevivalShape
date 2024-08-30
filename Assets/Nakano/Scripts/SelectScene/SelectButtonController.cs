using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択画面のボタン処理 UI表示
/// </summary>
public class SelectButtonController : MonoBehaviour
{
    // 注釈
    //「ステージ番号①」：10ステージ毎の番号。0のときはステージ1～10, 1のときはステージ11～20。
    //「ステージ番号②」：通常ステージに振られている番号。ステージ1～100。

    private int stageAmount = 0;

    Dictionary<int, bool> stageRelease = new(); // 解放状態
    Dictionary<int, bool> extraRelease = new();

    [SerializeField] private GameObject tutorialButton;

    [SerializeField, Header("ステージ選択画面①")] GameObject firstSelectPanel;
    [SerializeField, Header("ステージ選択画面①のボタン")] private Button[] buttons_FirstSelect;
    [SerializeField, Header("Content")] private RectTransform firstContent = null;

    [SerializeField] Scrollbar secondScrollbar;
    [SerializeField, Header("ステージ選択画面②")] GameObject secondSelectPanel;
    [SerializeField, Header("ステージ選択画面②")] private SelectButton selectButton;
    [SerializeField, Header("ステージ選択画面②のボタン")] private Button[] buttons_SecondSelect;
    [SerializeField] private Sprite[] missionIcons_sp;
    [SerializeField, Header("Content")] private RectTransform secondContent = null;

    [SerializeField, Header("エキストラボタンを引いた分のContentの縦幅")] private float contentHeight = 400.0f;
    [SerializeField] private int width;
    [SerializeField] private int maxHeight;

    bool loaded = false;

    public static int selectNumber = 0; // 選択したステージ番号①

    private const int needStarsForStageRelease = 25;
    private const int needStarsForExtraRelease = 30;

    void Start()
    {
        stageAmount = DataSave.GetStageAmount();

        // ステージの解放状態を初期化　一番最初の10ステージは解放済みにする
        // i = ステージ番号①
        for (int i = 0; i < stageAmount / 10; i++)
        {
            if (i == 0) stageRelease.Add(i, true);
            else stageRelease.Add(i, false);
            extraRelease.Add(i, false);
        }

        loaded = false;

        secondSelectPanel.SetActive(false);
    }

    private void Update()
    {
        // セーブデータの読み込み完了後、一度だけ呼び出し
        if (GameManager.DidLoad && !loaded)
        {
            loaded = true;

            // UIの設定
            FirstButtonsSetting();

            if (SceneName.GetLastSceneName() == "MainScene")
            {
                if (selectNumber >= 0) FirstSelect(selectNumber);
            }
        }
    }

    /// <summary>
    /// ステージ選択画面①の設定
    /// </summary>
    void FirstButtonsSetting()
    {
        firstSelectPanel.SetActive(true);

        for (int i = 0; i < stageAmount / 10; i++)
        {
            // 各ボタンの設定
            var fsb = buttons_FirstSelect[i].GetComponent<FirstSelectButton>();
            fsb.sbController = this;
            fsb.num = i;

            Release(i);

            // 解放済みのステージはボタンを押下できるようにする
            buttons_FirstSelect[i].interactable = stageRelease[i];
        }

        int num = 0;
        for (int i = 0; i < buttons_FirstSelect.Length; i++)
        {
            if(!buttons_FirstSelect[i].gameObject.activeSelf)
            {
                num++;
            }
        }
        var height = maxHeight - (num * contentHeight);
        firstContent.sizeDelta = new Vector2(width, height);

        // Tutorial
        if (GameManager.GetStageData("Tutorial") != null)
        {
            StageData data = GameManager.GetStageData("Tutorial");
            var imageParent = tutorialButton.transform.Find("Mission");
            MissionIconDisp(imageParent, data);
        }

        var ssb = tutorialButton.GetComponent<SecondSelectButton>();
        ssb.selectButton = selectButton;
        ssb.stageName = "Tutorial";
    }

    /// <summary>
    /// ステージ選択画面①に戻る
    /// </summary>
    public void Back()
    {
        firstSelectPanel.SetActive(true);
        secondSelectPanel.SetActive(false);
    }

    /// <summary>
    /// ステージ選択画面①のボタン押下時の処理
    /// </summary>
    /// <param name="num">押したボタンの番号</param>
    public void FirstSelect(int num)
    {
        selectNumber = num;
        firstSelectPanel.SetActive(false);
        secondSelectPanel.SetActive(true);

        SecondButtonsSetting(num);
    }

    /// <summary>
    /// ステージ選択画面②の設定
    /// </summary>
    /// <param name="num">選択したステージ番号①</param>
    void SecondButtonsSetting(int num)
    {
        // スクロールを初期位置に戻す
        secondScrollbar.value = 1;

        int undispButton = 0; // 非表示にするボタンの数

        for (int i = 0; i < buttons_SecondSelect.Length; i++)
        {
            string stageName = "";
            
            if (i == buttons_SecondSelect.Length - 1)
            {
                stageName = "Extra" + (num + 1).ToString();
                buttons_SecondSelect[i].interactable = extraRelease[num];

                // ベータ版ではExtra2～5は無し
                if(num >= 1)
                {
                    buttons_SecondSelect[i].gameObject.SetActive(false);
                    undispButton++;
                }
            }
            else
            {
                stageName = "Stage" + (num * 10 + i + 1).ToString();
            }

            // Text表示変更
            var childText = buttons_SecondSelect[i].transform.GetComponentInChildren<Text>();
            childText.text = stageName.ToUpper();

            // Extra、Tutorial以外はミッションクリアのアイコン表示を変更
            if(stageName.Contains("Stage"))
            {
                if (GameManager.GetStageData(stageName) != null)
                {
                    StageData data = GameManager.GetStageData(stageName);
                    var imageParent = buttons_SecondSelect[i].transform.Find("Mission");
                    MissionIconDisp(imageParent, data);
                }
            }
            
            var ssb = buttons_SecondSelect[i].GetComponent<SecondSelectButton>();
            ssb.selectButton = selectButton;
            ssb.stageName = stageName;
        }

        // Contentサイズ調整
        var height = maxHeight - (undispButton * contentHeight);
        secondContent.sizeDelta = new Vector2(width, height);
    }

    public void TutorialSelect()
    {
        selectNumber = -1;
    }

    void MissionIconDisp(Transform _parent, StageData _data)
    {
        for (int i = 0; i < 3; i++)
        {
            int spNum = _data.IsMissionClear[i] ? 1 : 0;
            _parent.GetChild(i).GetComponent<Image>().sprite = missionIcons_sp[spNum];
        }
    }

    /// <summary>
    /// ステージ解放状態の変更
    /// </summary>
    /// <param name="num">選択したステージ番号①</param>
    void Release(int num)
    {
        int starsAmount = 0;

        // 指定した10ステージの星獲得数を確認
        for(int i = 1; i <= 10; i++)
        {
            string stageName = "Stage" + (i + num).ToString();

            if(GameManager.GetStageData(stageName) != null)
                starsAmount += GameManager.GetStageData(stageName).GotStar;
        }

        // 次の10ステージを解放
        if (starsAmount >= needStarsForStageRelease)
        {
            if(num <= stageAmount) stageRelease[num + 1] = true;
        }
        // エクストラステージを解放
        if (starsAmount >= needStarsForExtraRelease)
        {
            extraRelease[num] = true;
        }
    }
}
