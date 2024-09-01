using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MissionScore : MonoBehaviour
{
    //仮のミッション -> プレイ結果　
    float time = 100f;
    int reConfirmation = 4;
    int missCount = 3;


    [SerializeField,Header("ミッションの秒数")] float[] missionTime;
    float judgeTime;
    [SerializeField, Header("ミッション再確認の回数")] int[] missionConfirmation;
    int judgeConfirmation;
    [SerializeField, Header("ミッションミスの回数")] int[] missionMiss;
    int judgeMiss;

    int scoreCount; //星の数
    string selectStage;

    public static int totalScore = 0;

    // 追加
    private string stageName;
    [SerializeField] private StageController stageController;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private Text timeText;
    [SerializeField] private Text reconfirmationText;
    [SerializeField] private Text missText;
    [SerializeField] private Text[] missionText;
    [SerializeField] private Image[] icons;
    [SerializeField] private Text[] missionText_menu;
    [SerializeField] private Image[] icons_menu;
    [SerializeField] private Sprite[] icons_sp;
    private bool[] isMissionClear = new bool[3];
    public bool[] IsMissionClear { get { return isMissionClear; } private set { isMissionClear = value; } }

    // Start is called before the first frame update
    void Start()
    {
        selectStageCheck();

        // 追加 ミッションクリアアイコンの非表示
        foreach(var i in icons)
        {
            i.enabled = false;
        }

        for (int i = 0; i < 3; i++)
        {
            isMissionClear[i] = false;
        }

        if (SelectButton.SelectStage != null)
            stageName = SelectButton.SelectStage;
        else stageName = stageController.StageName;

        MenuMissionSetting();
    }

    // Update is called once per frame
    void Update()
    {
        //リザルト画面が表示されたら
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    clearCheck();
        //}
        //if (Input.GetKeyDown(KeyCode.S))
        //{
        //    SceneManager.LoadScene("SelectScene");
        //}
    }

    //ステージごとのミッション条件を判定
    void selectStageCheck()
    {
        if(SelectButton.SelectStage != null) // nullチェック
            selectStage = SelectButton.SelectStage;
        else selectStage = stageController.StageName;

        //Stageの文字列を探す
        if (selectStage.Contains("Stage"))
        {
            //Stageを空白にする
            string stageName = selectStage.Replace("Stage","");
            //intとして持ってくる
            if (int.TryParse(stageName, out int s))
            {
                int sc = (s - 1) / 10;
                judgeTime = missionTime[sc];
                judgeConfirmation = missionConfirmation[sc];
                judgeMiss = missionMiss[sc];
                //Debug.Log(sc);

                // 追加 ミッション内容の表示
                missionText[0].text = "プレイ時間" + judgeTime.ToString() + "秒以内";
                missionText[1].text = "確認回数" + judgeConfirmation.ToString() + "回以内";
                missionText[2].text = "ミス回数" + judgeMiss.ToString() + "回以内";

                missionText_menu[0].text = "プレイ時間" + judgeTime.ToString() + "秒以内";
                missionText_menu[1].text = "確認回数" + judgeConfirmation.ToString() + "回以内";
                missionText_menu[2].text = "ミス回数" + judgeMiss.ToString() + "回以内";
            }
        }

        if(selectStage.ToLower() == "tutorial")
        {
            judgeTime = missionTime[0];
            judgeConfirmation = missionConfirmation[0];
            judgeMiss = missionMiss[0];

            // 追加 ミッション内容の表示
            missionText[0].text = "プレイ時間" + judgeTime.ToString() + "秒以内";
            missionText[1].text = "確認回数" + judgeConfirmation.ToString() + "回以内";
            missionText[2].text = "ミス回数" + judgeMiss.ToString() + "回以内";

            missionText_menu[0].text = "プレイ時間" + judgeTime.ToString() + "秒以内";
            missionText_menu[1].text = "確認回数" + judgeConfirmation.ToString() + "回以内";
            missionText_menu[2].text = "ミス回数" + judgeMiss.ToString() + "回以内";
        }
    }

    //スコアのカウント
    void clearCheck()
    {
        //かかった秒数
        //float rt = time - judgeTime;
        float rt = judgeTime - time; // 残り時間が0よりも多かったらに変更
        if (rt > 0)
        {
            scoreCount++;
            isMissionClear[0] = true;
        }
        else isMissionClear[0] = false;
        //再確認の回数
        int rc= reConfirmation - judgeConfirmation;
        if (rc < 0) // ～回以内なので=を削除
        {
            scoreCount++;
            isMissionClear[1] = true;
        }
        else isMissionClear[1] = false;
        //失敗した回数
        int cm = missCount - judgeMiss;
        if (cm < 0)
        {
            //Debug.Log("test");
            scoreCount++;
            isMissionClear[2] = true;
        }
        else isMissionClear[2] = false;

        totalScore += scoreCount;
        //Debug.Log(totalScore);

        // データ保存
        GameManager.StageDataUpdate(stageName, true, isMissionClear);
    }

    // 追加　UIの表示・数値の入力
    public void ResultDisp()
    {
        time = Mathf.FloorToInt(timeManager.TotalTime);
        //Debug.Log(time);
        reConfirmation = stageController.Reconfirmation;
        missCount = stageController.Miss;

        int min = Mathf.FloorToInt(time / 60.0f);
        //Debug.Log(min);
        int sec = Mathf.FloorToInt(time - (min * 60.0f));
        timeText.text = min.ToString("d2") + ":" + sec.ToString("d2");
        reconfirmationText.text = reConfirmation.ToString();
        missText.text = missCount.ToString();

        clearCheck();
    }

    void MenuMissionSetting()
    {
        for (int i = 0; i < 3; i++)
        {
            if (!stageName.Contains("Stage") && !stageName.Contains("Tutorial"))
            {
                missionText_menu[i].enabled = false;
                icons_menu[i].enabled = false;
            }

            if (GameManager.GetStageData(stageName) != null)
                icons_menu[i].sprite = GameManager.GetStageData(stageName).IsMissionClear[i] ? icons_sp[1] : icons_sp[0];
        }
    }
}
