using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionScore : MonoBehaviour
{
    //仮のミッション
    float time = 80f;
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
    // Start is called before the first frame update
    void Start()
    {
        selectStageCheck();
    }

    // Update is called once per frame
    void Update()
    {
        //リザルト画面が表示されたら
        if (Input.GetKeyDown(KeyCode.A))
        {
            clearCheck();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene("SelectScene");
        }
    }

    //ステージごとのミッション条件を判定
    void selectStageCheck()
    {
        selectStage = SelectButton.SelectStage;

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
                Debug.Log(sc);
            }
        }
    }

    //スコアのカウント
    void clearCheck()
    {
        //かかった秒数
        float rt = time - judgeTime;
        if (rt <= 0)
        {
            scoreCount++;
        }
        //再確認の回数
        int rc= reConfirmation - judgeConfirmation;
        if (rc <= 0)
        {
            scoreCount++;
        }
        //失敗した回数
        int cm = missCount - judgeMiss;
        if (cm <= 0)
        {
            scoreCount++;
        }
        totalScore += scoreCount;
        Debug.Log(totalScore);
    }
}
