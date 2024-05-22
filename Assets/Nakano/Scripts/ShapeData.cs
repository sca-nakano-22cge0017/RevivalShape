using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 図形
/// </summary>
public class ShapeData : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    
    /// <summary>
    /// オブジェクトの形
    /// </summary>
    public enum Shape { Empty, Cube, Sphere, };

    /// <summary>
    /// string型からShapeに変換
    /// </summary>
    /// <param name="s">変換文字列</param>
    /// <returns>文字列に対応した形を返す</returns>
    public Shape StringToShape(string s)
    {
        Shape shape = new();
        string str = s.ToLower(); // 小文字に変換

        switch (str)
        {
            case "c":
                shape = Shape.Cube;
                break;

            case "s":
                shape = Shape.Sphere;
                break;

            case "e":
            case " ":
                shape = Shape.Empty;
                break;
            default:
                shape = Shape.Empty;
                break;
        }

        return shape;
    }

    /// <summary>
    /// Shapeから生成するオブジェクトを判定・返す
    /// </summary>
    /// <param name="s">Shape</param>
    /// <returns>生成オブジェクト（Prefab）</returns>
    public GameObject ShapeToPrefabs(Shape s)
    {
        int objNum;

        switch (s)
        {
            case Shape.Empty:
                objNum = 0;
                break;
            case Shape.Cube:
                objNum = 1;
                break;
            case Shape.Sphere:
                objNum = 2;
                break;
            default:
                objNum = 0;
                break;
        }

        return prefabs[objNum];
    }

    /// <summary>
    /// 三次配列内に出てくる図形の種類を調べる
    /// </summary>
    /// <param name="checkArray">調べたい配列</param>
    /// <returns>使用図形の種類を配列で返す Emptyは飛ばす</returns>
    public Shape[] ShapeTypes(Shape[,,] checkArray)
    {
        Shape[] type = new Shape[System.Enum.GetValues(typeof(Shape)).Length];

        for (int x = 0; x < checkArray.GetLength(0); x++)
        {
            for (int y = 0; y < checkArray.GetLength(1); y++)
            {
                for (int z = 0; z < checkArray.GetLength(2); z++)
                {
                    for (int t = 0; t < type.Length; t++)
                    {
                        // Empty、確認済みの図形なら次のマスへ
                        if (checkArray[x, y, z] == Shape.Empty || type[t] == checkArray[x, y, z])
                            break;

                        // 既に値が入っていたら次の要素へ
                        if (type[t] != Shape.Empty)
                            continue;

                        // 未確認の図形なら配列に追加する
                        if (checkArray[x, y, z] != Shape.Empty && type[t] == Shape.Empty)
                        {
                            type[t] = checkArray[x, y, z];
                            break;
                        }
                    }
                }
            }
        }

        return type;
    }

    /// <summary>
    /// 配列内の図形種類数を計算
    /// </summary>
    /// <param name="checkArray">調べる配列</param>
    /// <returns>配列内の図形種類数</returns>
    public int ShapeTypesAmount(Shape[] checkArray)
    {
        int num = 0;

        foreach(var s in checkArray)
        {
            if(s != Shape.Empty)
                num++;
        }

        return num;
    }
}
