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
    private static string dataPath = " "; // ファイルパス

    private static int stageAmount;
    /// <summary>
    /// ステージ数取得
    /// </summary>
    /// <returns>チュートリアルやエクストラを含めないステージ数</returns>
    public static int GetStageAmount() { return stageAmount; }
    public static void SetStageAmount(int _stageAmount) { if (_stageAmount > 0) stageAmount = _stageAmount; }

    /// <summary>
    /// ファイルのパスを取得する
    /// </summary>
    /// <returns>ファイルのパス</returns>
    public static bool SetDataPath()
    {
        string _dataPath;

        //パスを指定して読み込み
        string directoryName = Application.persistentDataPath + "/Resources";

        //ディレクトリがなかったら
        while (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName); //生成
        }
#if UNITY_EDITOR
        _dataPath = Application.dataPath + "/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        _dataPath = Application.persistentDataPath + "/Resources/AndroidJson.json";
#endif

        if (_dataPath == null || _dataPath == " ")
        {
            Debug.Log("ファイルのパスが取得できませんでした。");
        }

        dataPath = _dataPath;

        return true;
    }

    /// <summary>
    /// ファイルの存在確認
    /// </summary>
    /// <returns>確認完了</returns>
    public static bool FileCheck()
    {
        //Jsonファイルがなければ生成、初期化
        while (!File.Exists(dataPath))
        {
            //ファイル生成
            FileStream fs = File.Create(dataPath);
            fs.Close();
            Initialize();
        }

        return true; // ファイル確認完了
    }

    /// <summary>
    /// データ初期化
    /// </summary>
    public static void Initialize()
    {
        PlayerData data = new PlayerData();

        // ステージ総数分初期化
        data.DataList = new();
        for (int i = 0; i <= stageAmount; i++)
        {
            StageData stageData = new StageData();

            if (i == 0) stageData.StageName = "Tutorial"; // 一番最初にチュートリアルのデータを入力
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

    /// <summary>
    /// セーブ
    /// </summary>
    /// <param name="_playerData">データ</param>
    public static void SavePlayerData(PlayerData _playerData)
    {
        StreamWriter writer = null;

        //playerデータをJsonに変換
        string jsonstr = JsonUtility.ToJson(_playerData);

        //Jsonファイルに書き込み
        writer = new StreamWriter(dataPath, false);
        writer.Write(jsonstr);
        writer.Flush();
        writer.Close();
    }

    /// <summary>
    /// ロード
    /// </summary>
    /// <returns>クリアデータ</returns>
    public static PlayerData LoadPlayerData()
    {
        string datastr = "";
        StreamReader reader;
        reader = new StreamReader(dataPath);
        datastr = reader.ReadToEnd();
        reader.Close();

        //jsonファイルをオブジェクトに変更
        return JsonUtility.FromJson<PlayerData>(datastr);
    }

    /// <summary>
    /// データリセット
    /// </summary>
    public static void DataReset()
    {
        if (File.Exists(dataPath))
        {
            File.Delete(dataPath);
        }

        FileCheck();
    }
}