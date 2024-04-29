using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeData : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    
    /// <summary>
    /// �I�u�W�F�N�g�̌`
    /// </summary>
    public enum Shape { Empty, Cube, Sphere, };

    /// <summary>
    /// string�^����Shape�ɕϊ�
    /// </summary>
    /// <param name="s">�ϊ�������</param>
    /// <returns>������ɑΉ������`��Ԃ�</returns>
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
    /// Shape���琶������I�u�W�F�N�g�𔻒�E�Ԃ�
    /// </summary>
    /// <param name="s">Shape</param>
    /// <returns>�����I�u�W�F�N�g�iPrefab�j</returns>
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
