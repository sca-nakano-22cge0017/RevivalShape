using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StageDataLoader : MonoBehaviour
{
    string folderName = "/Nakano/StageData";

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

    public List<List<List<StageCreate.Shape>>> LoadStage(string stageName)
    {
        string fileName = Application.dataPath + folderName + "/" + stageName + ".csv";
        string dataStr = "";

        FileCheck(fileName); // 存在確認

        Vector3 size = new Vector3(0,0,0);
        List<List<List<StageCreate.Shape>>> map = new();

        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

        string[] line = dataStr.Split("\n"); // 改行で分割

        for(int z = 0; z < line.Length; z++)
        {
            // スキップしたい文章（説明文や補足）に来たら終わりにする
            if (line[0][0] == '!') break;

            var cell = line[z].Split(","); // コンマで区切る セルごとに分かれる
            
            if(z == 0) // 1行目に書いてある幅・奥行・高さを代入
            {
                if (int.TryParse(cell[0], out int w)) size.x = w;
                if (int.TryParse(cell[1], out int d)) size.z = d;
                if (int.TryParse(cell[2], out int h)) size.y = h;
            }

            else
            {
                // 3次元のリストに代入
                List<List<StageCreate.Shape>> zLine = new();

                for (int x = 0; x < cell.Length; x++)
                {
                    var obj = cell[x].Split("/"); // スラッシュで区切る

                    List<StageCreate.Shape> xCell = new();

                    for (int y = 0; y < size.z; y++)
                    {
                        // 配列からデータが漏れないように調整する
                        int h = (int)size.z > obj.Length ? (int)size.z : obj.Length;
                        string[] o = new string[h];
                        
                        // 指定のデータ数に足りなかったらデータ数を追加補正
                        if(obj.Length <= y) { o[y] = ""; }
                        else o[y] = obj[y];

                        // 生成する図形を代入
                        StageCreate.Shape s = StringToShape(o[y]);
                        xCell.Add(s);
                    }

                    zLine.Add(xCell);
                }

                map.Add(zLine);
            }
        }

        return map;
    }

    StageCreate.Shape StringToShape(string s)
    {
        StageCreate.Shape shape = new();

        switch (s)
        {
            case "C":
            case "c":
                shape = StageCreate.Shape.Cube;
                break;

            case "S":
            case "s":
                shape = StageCreate.Shape.Sphere;
                break;

            case "E":
            case "e":
            case "":
                shape = StageCreate.Shape.Empty;
                break;
        }

        return shape;
    }
}
