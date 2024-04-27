using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageCreate : MonoBehaviour
{
    [SerializeField] GameObject[] prefabs;
    [SerializeField] StageDataLoader stageDataLoader;

    public enum Shape { Empty, Cube, Sphere, Cylinder, };
    public Shape shape;

    List<List<List<StageCreate.Shape>>> map = new(); // ステージ情報　奥行・幅・高さの順

    int width = 4, depth = 4, height = 4;

    void Start()
    {
        map = stageDataLoader.LoadStage("StageDataSample");

        ObjInstance();
    }

    void Update()
    {
        
    }

    void ObjInstance()
    {
        for (int z = 0; z < depth; z++)
        {
            List<List<StageCreate.Shape>> line = map[z];

            for (int x = 0; x < width; x++)
            {
                List<StageCreate.Shape> c = line[x];

                for (int y = 0; y < height; y++)
                {
                    Shape s = c[y];

                    Vector3 pos = new Vector3(x, y, z);

                    Instantiate(ShapeCheck(s), pos, Quaternion.identity);
                }
            }
        }
    }

    GameObject ShapeCheck(Shape s)
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
            case Shape.Cylinder:
                objNum = 3;
                break;
            default:
                objNum = 0;
                break;
        }

        return prefabs[objNum];
    }
}
