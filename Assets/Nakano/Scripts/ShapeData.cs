using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

        switch (s)
        {
            case "C":
            case "c":
                shape = Shape.Cube;
                break;

            case "S":
            case "s":
                shape = Shape.Sphere;
                break;

            case "E":
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
}
