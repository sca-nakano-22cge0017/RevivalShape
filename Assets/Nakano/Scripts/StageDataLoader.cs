using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// CSVファイルを読み込み、配置データに変換する
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    // Addressable
    string mapSizeDataFile = "MapSizeData";
    string mapSizeDataText;
    string stageDataText;

    [HideInInspector] public bool mapSizeDataLoadComlete = false;
    [HideInInspector] public bool stageDataLoadComlete = false;

    [SerializeField] ShapeData shapeData;

    private void Awake()
    {
        AsyncOperationHandle<TextAsset> m_TextHandle;

        // マップサイズのデータを取得
        Addressables.LoadAssetAsync<TextAsset>(mapSizeDataFile).Completed += handle => {
            m_TextHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }
            mapSizeDataText = handle.Result.text;
            mapSizeDataLoadComlete = true;
        };
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

        for (int z = 0; z < mapSize.z; z++)
        {
            // スキップしたい文章（説明文や補足）に来たら終わりにする
            if (line[z].StartsWith("!")) break;

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

                    map[x, y, z] = shapeData.StringToShape(o[y]);
                }
            }
        }

        return map;
    }

    /// <summary>
    /// ステージの配置データをロードする
    /// </summary>
    /// <param name="stageName">ステージ名</param>
    public void StageDataGet(string stageName)
    {
        AsyncOperationHandle<TextAsset> m_TextHandle;

        // マップサイズのデータを取得
        Addressables.LoadAssetAsync<TextAsset>(stageName).Completed += handle => {
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
    /// ステージのマップサイズデータを取得する
    /// </summary>
    /// <param name="stageName">プレイステージ</param>
    /// <returns>サイズをVector3で返す</returns>
    public Vector3 LoadStageSize(string stageName)
    {
        Vector3 mapSize = new Vector3(0, 0, 0);

        string dataStr = mapSizeDataText;

        string[] line = dataStr.Split("\n"); // 改行で分割

        for (int z = 0; z < line.Length; z++)
        {
            if(z == 0) continue; // スキップしたい文章（説明文や補足）に来たら / 1行目を飛ばす
            if(line[z][0] == '!' || line[z][0] == '！') break;

            var cell = line[z].Split(","); // コンマで区切る セルごとに分かれる

            // ステージ名と同じになるまでやり直す 大文字小文字を区別しない
            if(string.Compare(stageName, cell[0], true) != 0) continue;

            // マップのサイズを代入
            if (int.TryParse(cell[1], out int w)) mapSize.x = w;
            if (int.TryParse(cell[2], out int d)) mapSize.z = d;
            if (int.TryParse(cell[3], out int h)) mapSize.y = h;
        }

        return mapSize;
    }
}