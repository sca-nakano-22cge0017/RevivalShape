using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

//Playerのクリア状態を保存する
[System.Serializable]
public class PlayerData
{
    public List<StageData> DataList = new();
}

[System.Serializable]
public class StageData
{
    // 追加
    public string StageName; // ステージ名

    public bool IsClear; //クリアしているかどうか
    public int GotStar; //星の数(ミッションクリア状態)

    // 追加
    public bool[] IsMissionClear = new bool[3]; // 各ミッションクリア状況
}

public class DataSave : MonoBehaviour
{
    string datapath;

    PlayerData playerData;

    // 追加
    [SerializeField] private GameManager gameManager;
    private int stageAmount = 0; // チュートリアルやエクストラを含めないステージ数
    private bool didFileChecked = false; // ファイル確認済みか
    public bool DidFileChecked { get { return didFileChecked; } private set { didFileChecked = value; } }

    private void Awake()
    {
        stageAmount = gameManager.StageAmount; // ステージ数取得

        //パスを指定して読み込み
        string directoryName = Application.persistentDataPath + "/Resources";

        //ディレクトリがなかったら
        while (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName); //生成
        }
#if UNITY_EDITOR
        datapath = Application.dataPath + "/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        datapath = Application.persistentDataPath + "/Resources/AndroidJson.json";
#endif
    }

    //セーブ
    public void SavePlayerData(PlayerData player)
    {
        StreamWriter writer = null;

        //playerデータをJsonに変換
        string jsonstr = JsonUtility.ToJson(player);

        //Jsonファイルに書き込み
        writer = new StreamWriter(datapath, false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }

    //Jsonファイルを読み込み、ロードする
    public PlayerData LoadPlayerData()
    {
        string datastr = "";
        StreamReader reader;
        reader = new StreamReader(datapath);
        datastr = reader.ReadToEnd();
        reader.Close();

        //jsonファイルをオブジェクトに変更
        return JsonUtility.FromJson<PlayerData> (datastr);
    }

    //Jsonファイルがない場合初期値をセーブし生成する
    public void Initialize()
    {
        PlayerData data = new PlayerData();
        //StageData stageData = new StageData();

        //stageData.IsClear = false;
        //stageData.GotStar = 0;

        ////リストの中にデータ生成
        //data.DataList.Add(stageData);

        // 追加 ステージ総数分初期化
        data.DataList = new();
        for (int i = 0; i < stageAmount; i++)
        {
            StageData stageData = new StageData();

            if(i == 0) stageData.StageName = "Tutorial"; // 一番最初にチュートリアルのデータを入力
            else stageData.StageName = "Stage" + i.ToString();
            
            stageData.IsClear = false;
            stageData.GotStar = 0;
            
            for (int m = 0; m < 3; m++)
            {
                stageData.IsMissionClear[m] = false;
            }

            data.DataList.Add(stageData);
        }

        var extraAmount = stageAmount / 10; // エクストラステージの数
        for (int i = 1; i <= extraAmount; i++)
        {
            StageData stageData = new StageData();

            stageData.StageName = "Extra" + i.ToString();

            stageData.IsClear = false;
            stageData.GotStar = 0;

            for (int m = 0; m < 3; m++)
            {
                stageData.IsMissionClear[m] = false;
            }

            data.DataList.Add(stageData);
        }

        // Tutorial -> Stage1～100 -> Extra1～20の順で保存

        SavePlayerData(data);
    }

    //ファイルリセット
    public void RestartButton()
    {
        Debug.Log("test" + File.Exists(datapath));
        if (File.Exists(datapath))
        {
            File.Delete(datapath);
        }

        FileCheck();

        gameManager.DataReset(); // GameManager内のデータを書き換え
    }


    /// <summary>
    /// ファイルの存在確認
    /// </summary>
    public void FileCheck()
    {
        //Jsonファイルがなければ生成、初期化
        // Todo Jsonファイルの形式が正しくなければファイルを生成し直す
        while (!File.Exists(datapath))
        {
            //ファイル生成
            FileStream fs = File.Create(datapath);
            fs.Close();
            Initialize();
        }

        didFileChecked = true; // ファイル確認完了
    }
}
