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
    public bool IsClear; //クリアしているかどうか
    public int GotStar; //星の数(ミッションクリア状態)
}

public class DataSave : MonoBehaviour
{
    string datapath;

    PlayerData playerData;

    private void Awake()
    {
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
        //Jsonファイルがなければ生成、初期化
        while (!File.Exists(datapath))
        {
            //ファイル生成
            FileStream fs = File.Create(datapath);
            fs.Close();
            Initialize();
        }

        //Jsonファイルがあればロード
        if (File.Exists(datapath))
        {
            playerData = LoadPlayerData();
        }
        else
        {
            Debug.Log("ファイルが存在しません");
        }
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
        StageData stageData = new StageData();

        stageData.IsClear = false;
        stageData.GotStar = 0;

        //リストの中にデータ生成
        data.DataList.Add(stageData);

        SavePlayerData(data);
    }
}
