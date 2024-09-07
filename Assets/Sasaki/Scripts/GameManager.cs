using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class GameManager : MonoBehaviour
{
    public static PlayerData playerData = null;

    [SerializeField, Header("チュートリアルやエクストラを含めないステージ数")] private int stageAmount = 50;
    [SerializeField, Header("フレームレート")] int fps = 120;

    private const int missionAmount = 3; // ミッション数

    private static bool didLoad = false; // データロードしたか
    public static bool DidLoad
    {
        get { return didLoad; }
        private set { }
    }

    // シングルトン
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        FpsSet();
        DataSave.SetStageAmount(stageAmount);
    }

    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    private void Update()
    {
        // ファイルが存在するかの確認が完了したら
        if (DataSave.SetDataPath() && DataSave.FileCheck() && !didLoad)
        {
            while (DataSave.LoadPlayerData() == null)
            {
                // データが無ければ初期化
                DataSave.Initialize();
            }

            playerData = DataSave.LoadPlayerData();
            didLoad = true; // 読み込み完了
        }
    }

    void FpsSet()
    {
        Application.targetFrameRate = fps;
    }

    /// <summary>
    /// データリセット
    /// </summary>
    public static void DataReset()
    {
        DataSave.DataReset();

        // 再読み込み
        didLoad = false;
    }

    /// <summary>
    /// ステージ全開放
    /// </summary>
    public static void AllRelease()
    {
        bool[] mission = { true, true, true };
        StageDataUpdate("Tutorial", true, mission);

        for (int i = 1; i <= DataSave.GetStageAmount(); i++)
        {
            StageDataUpdate("Stage" + i.ToString(), true, mission);
        }
    }

    /// <summary>
    /// ステージのクリア状況等を更新・保存
    /// </summary>
    /// <param name="stageName"></param>
    public static void StageDataUpdate(string _stageName, bool _isClear, bool[] _isMissionClear)
    {
        // 今回保存したステージの名前と、ステージ名が一致するDataListの要素を取得する
        foreach (var d in playerData.DataList)
        {
            if (_stageName.ToLower() == d.StageName.ToLower())
            {
                // データ入力
                d.IsClear = _isClear;

                for (int i = 0; i < missionAmount; i++)
                {
                    if (!d.IsMissionClear[i])
                    {
                        d.IsMissionClear[i] = _isMissionClear[i];
                    }
                    
                    if (_isMissionClear[i] == true && d.GotStar < 3)
                    {
                        d.GotStar++;
                    }
                }
            }
        }

        // セーブ
        DataSave.SavePlayerData(playerData);
    }

    /// <summary>
    /// 指定したステージのデータを取得する
    /// </summary>
    /// <param name="_stageName">ステージ名</param>
    /// <returns>ステージデータ</returns>
    public static StageData GetStageData(string _stageName)
    {
        StageData data = new();

        foreach (var d in playerData.DataList)
        {
            if (_stageName.ToLower() == d.StageName.ToLower())
            {
                data = d;
            }
        }

        return data;
    }
}