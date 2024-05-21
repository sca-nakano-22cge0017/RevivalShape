using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

// TutorialやExtraStageなどの「Stage + 数字」で構成されていないステージ名の場合は、
// ステージ名と配置データがあるファイル名を個別に設定する
[System.Serializable]
public class StageFile
{
    public string stageName;
    public string fileName;
}

/// <summary>
/// CSVファイルを読み込み、配置データに変換する
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    // Addressable
    string stageDataText;

    [HideInInspector] public bool stageDataLoadComlete = false;

    [SerializeField] ShapeData shapeData;
    [SerializeField, Header("1ファイル内のステージ数 10ステージ分記述なら10")] int StageAmountPerFile;
    [SerializeField, Header("ステージ毎のファイル名の指定"),
        Tooltip("TutorialやExtraStageなどの「Stage + 数字」で名前が構成されていないステージのデータがどのファイルにあるかを指定する")] StageFile[] fileSelect;
    int fileNum;
    string fileName;
    string fileNameFormat1 = "Assets/Nakano/StageData/StageData";
    string fileNameFormat2 = ".csv";

    bool isNumberStage = true; // 「Stage + 数字」で名前が構成されるステージか

    private string selectStageName; // 選択ステージ名
    
    /// <summary>
    /// ステージの配置データをロードする
    /// </summary>
    /// <param name="stageName">ステージ名</param>
    public void StageDataGet(string stageName)
    {
        selectStageName = stageName;

        LoadFileSelect(stageName);

        AsyncOperationHandle<TextAsset> m_TextHandle;

        // マップサイズのデータを取得
        Addressables.LoadAssetAsync<TextAsset>(fileName).Completed += handle => {
            m_TextHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }
            stageDataText = handle.Result.text;
            stageDataLoadComlete = true;
        };
    }

    /// <summary>
    /// ステージのマップサイズデータと使用図形数を取得する
    /// </summary>
    /// <param name="stageName">プレイステージ</param>
    /// <returns>サイズをVector3で、使用図形数をintで返す</returns>
    public Vector3 LoadStageSize()
    {
        Vector3 mapSize = new Vector3(5, 5, 5);

        string dataStr = stageDataText;

        string[] line = dataStr.Split("\n"); // 改行で分割

        for(int l = 0; l < line.Length; l++)
        {
            if (l + 1 > line.Length) break;

            string[] name = line[l].Split(",");
            if (name[0].ToLower() != selectStageName.ToLower()) continue;

            string[] sizeText = line[l+1].Split(","); // ステージ名の次の行にサイズ情報
            
            if (int.TryParse(sizeText[0], out int w)) mapSize.x = w;
            if (int.TryParse(sizeText[1], out int h)) mapSize.y = h;
            if (int.TryParse(sizeText[2], out int d)) mapSize.z = d;
            break;
        }

        return mapSize;
    }

    /// <summary>
    /// オブジェクトの配置データ取得
    /// </summary>
    /// <param name="stageName">プレイステージ</param>
    /// <returns>三次元配列の配置データを返す</returns>
    public ShapeData.Shape[,,] LoadStageMap(Vector3 mapSize)
    {
        string dataStr = stageDataText;

        // データ配列
        ShapeData.Shape[,,] map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        string[] line = dataStr.Split("\n"); // 改行で分割

        int mapDataBegin = 0; // 指定ステージの配置データの一番最初のデータの位置

        for (int l = 0; l < line.Length; l++)
        {
            string[] name = line[l].Split(",");
            if (name[0].ToLower() != selectStageName.ToLower()) continue;

            mapDataBegin = l + 2;

            break;
        }

        for (int z = mapDataBegin; z < mapSize.z + mapDataBegin; z++)
        {
            // スキップしたい文章（説明文や補足）に来たら飛ばす
            if (line[z].StartsWith("!")) continue;

            // MapSizeDataで指定したDepthよりデータ数が少なかった場合は補完
            string[] l = DataComplement((int)mapSize.z, line.Length, line, z);

            var cell = l[z].Split(","); // コンマで区切る セルごとに分かれる

            for (int x = 0; x < mapSize.x; x++)
            {
                // MapSizeDataで指定したWidthよりデータ数が少なかった場合は補完
                string[] c = DataComplement((int)mapSize.x, cell.Length, cell, x);

                var obj = c[x].Split("/"); // スラッシュで区切る

                for (int y = 0; y < mapSize.y; y++)
                {
                    // MapSizeDataで指定したHeightよりデータ数が少なかった場合は補完
                    string[] o = DataComplement((int)mapSize.y, obj.Length, obj, y);

                    map[x, y, z - mapDataBegin] = shapeData.StringToShape(o[y]);
                }
            }
        }

        return map;
    }

    /// <summary>
    /// データ補完
    /// </summary>
    /// <param name="idealDataSize">MapSizeDataで指定したデータ数　mapSize</param>
    /// <param name="actualDataSize">実際にあるデータ数　string[]の要素数</param>
    /// <param name="data">配置データが入っているstring[]</param>
    /// <param name="dataNum">配列の何番目か</param>
    /// <returns></returns>
    string[] DataComplement(int idealDataSize, int actualDataSize, string[] data, int dataNum) 
    {
        // MapSizeDataで指定したデータ数より、実際のデータ数が少なかった場合は補完
        int n = idealDataSize > actualDataSize ? idealDataSize : actualDataSize;
        string[] s = new string[n];
        s[dataNum] = actualDataSize <= dataNum ? s[dataNum] = "" : data[dataNum];
        return s;
    }

    /// <summary>
    /// ステージ名に応じて読み込むファイルを変える
    /// </summary>
    /// <param name="stageName">ステージ名</param>
    void LoadFileSelect(string stageName)
    {
        if(stageName.Contains("Stage"))
        {
            string _stageName = stageName.Replace("Stage", "");

            if (int.TryParse(_stageName, out int n))
            {
                // ステージの番号に応じてファイル名取得
                fileNum = ((n - 1) / StageAmountPerFile + 1);
                fileName = fileNameFormat1 + fileNum.ToString() + fileNameFormat2;

                return;
            }

            else isNumberStage = false;
        }
        else isNumberStage = false;

        // ステージ名が番号じゃない場合（TutorialやExtraStageの場合）は指定ファイルを読み込み
        if (!isNumberStage)
        {
            for (int i = 0; i < fileSelect.Length; i++)
            {
                if (fileSelect[i].stageName.ToLower() == stageName.ToLower())
                {
                    fileName = "Assets/Nakano/StageData/" + fileSelect[i].fileName;
                }
            }

            return;
        }
    }
}