using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �}�`
/// </summary>
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
        string str = s.ToLower(); // �������ɕϊ�

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

    /// <summary>
    /// �O���z����ɏo�Ă���}�`�̎�ނ𒲂ׂ�
    /// </summary>
    /// <param name="checkArray">���ׂ����z��</param>
    /// <returns>�g�p�}�`�̎�ނ�z��ŕԂ� Empty�͔�΂�</returns>
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
                        // Empty�A�m�F�ς݂̐}�`�Ȃ玟�̃}�X��
                        if (checkArray[x, y, z] == Shape.Empty || type[t] == checkArray[x, y, z])
                            break;

                        // ���ɒl�������Ă����玟�̗v�f��
                        if (type[t] != Shape.Empty)
                            continue;

                        // ���m�F�̐}�`�Ȃ�z��ɒǉ�����
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
    /// �z����̐}�`��ސ����v�Z
    /// </summary>
    /// <param name="checkArray">���ׂ�z��</param>
    /// <returns>�z����̐}�`��ސ�</returns>
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
