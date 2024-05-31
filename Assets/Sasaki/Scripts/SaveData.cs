using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.AddressableAssets.Build;

[System.Serializable]
public class PlayerData
{
    public bool clearCheck; //クリアしているかどうか
    public int star;
}

public class SaveData : MonoBehaviour
{
    string datapath;

    private void Awake()
    {
#if UNITY_EDITOR
        datapath = Application.dataPath + "Assets/Resources/WinJson.json";
#endif
#if UNITY_ANDROID
        datapath = Application.persistentDataPath + "Assets/Resources/AndJson.json";
#endif
    }

    private void Start()
    {
        //Jsonファイルがあればロード、なければ初期化
        PlayerData player = new PlayerData();
        if (File.Exists(datapath))
        {
            player = LoadPlayerData();
        }
        else
        {
            Initialize(player);
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
    public void Initialize(PlayerData player)
    {
        player.clearCheck = false;
        player.star = 0;

        SavePlayerData(player);
    }
}
