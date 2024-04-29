using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// CSVファイルを読み込み、配置データに変換する
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    [SerializeField] ShapeData shapeData;

    string folderName = "/Nakano/StageData";
    string mapSizeFile = "/MapSizeData.csv";

    /// <summary>
    /// ファイル・ディレクトリの存在確認
    /// </summary>
    /// <param name="fileName">ファイル名</param>
    /// <param name="length">ランキングデータの配列数</param>
    void FileCheck(string name)
    {
        string directoryName = Application.dataPath + folderName;
        string fileName = name;

        while (!Directory.Exists(directoryName)) //ディレクトリがなかったら
        {
            Directory.CreateDirectory(directoryName); //ディレクトリを作成
        }

        while (!File.Exists(fileName)) // ファイルがなかったら
        {
            FileStream fs = File.Create(fileName); // ファイルを作成
            fs.Close(); // ファイルを閉じる
        }
    }

    /// <summary>
    /// オブジェクトの配置データ取得
    /// </summary>
    /// <param name="stageName">プレイステージ</param>
    /// <returns>三次元の配置データを返す</returns>
    public ShapeData.Shape[,,] LoadStageMap(string stageName)
    {
        string fileName = Application.dataPath + folderName + "/" + stageName + ".csv";
        string dataStr = "";

        FileCheck(fileName); // 存在確認

        // マップのサイズを取得
        Vector3 mapSize = LoadStageSize(stageName);

        // データ配列
        ShapeData.Shape[,,] map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // 読み込み
        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

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

        string fileName = Application.dataPath + folderName + mapSizeFile;
        string dataStr = "";

        FileCheck(fileName); // 存在確認

        // 読み込み
        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

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