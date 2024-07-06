using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private PlayerData playerData = null;
    [SerializeField] private DataSave dataSave;

    // 追加
    [SerializeField, Header("チュートリアルやエクストラを含めないステージ数")] private int stageAmount = 20;
    public int StageAmount { get { return stageAmount; } private set { } }
    private int missionAmount = 3; // ミッション数
    private bool didLoad = false; // データロードしたか
    public bool DidLoad { get { return didLoad;} set { } }

    [SerializeField, Header("フレームレート")] int fps = 120;

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
    }

    private void Start()
    {
        Application.targetFrameRate = fps;
        DontDestroyOnLoad(this);

        if (dataSave != null)
        {
            dataSave.FileCheck();
        }
    }

    private void Update()
    {
        // ファイルが存在するかの確認が完了したら
        if (dataSave.DidFileChecked && !didLoad)
        {
            while (dataSave.LoadPlayerData() == null)
            {
                //playerData.DataList = new();

                // データが無ければ初期化
                dataSave.Initialize();
            }

            while (playerData == null)
            {
                playerData = dataSave.LoadPlayerData();
            }

            didLoad = true; // 読み込み完了
        }

        if(Input.GetKeyDown(KeyCode.Delete))
        {
            dataSave.Initialize();
        }
    }

    /// <summary>
    /// ステージのクリア状況等を更新・保存
    /// </summary>
    /// <param name="stageName"></param>
    public void StageDataUpdate(string _stageName, bool _isClear, bool[] _isMissionClear)
    {
        // 今回保存したステージの名前と、ステージ名が一致するDataListの要素を取得する
        foreach (var d in playerData.DataList)
        {
            if(_stageName.ToLower() == d.StageName.ToLower())
            {
                // データ入力
                d.IsClear = _isClear;

                for (int i = 0; i < missionAmount; i++)
                {
                    d.IsMissionClear[i] = _isMissionClear[i];

                    if(_isMissionClear[i] == true && d.GotStar < 3)
                    {
                        d.GotStar++;
                    }
                }
            }
        }

        // セーブ
        dataSave.SavePlayerData(playerData);
    }

    /// <summary>
    /// 指定したステージのデータを取得する
    /// </summary>
    /// <param name="_stageName">ステージ名</param>
    /// <returns>ステージデータ</returns>
    public StageData GetStageData(string _stageName)
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
