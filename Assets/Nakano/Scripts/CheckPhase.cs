using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckPhase : MonoBehaviour
{
    [SerializeField] ShapeData shapeData;
    [SerializeField] StageController stageController;

    [SerializeField] Transform objParent;

    [SerializeField] GameObject checkPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    bool sampleCreated = false;

    private void Awake()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// �m�F�t�F�[�Y�ڍs���̏���
    /// </summary>
    public void CheckPhaseStart()
    {
        checkPhaseUI.SetActive(true);
        objParent.gameObject.SetActive(true);

        // �I�u�W�F�N�g����
        if (!sampleCreated) StageInstance();
    }

    /// <summary>
    /// �m�F�t�F�[�Y�I��
    /// </summary>
    public void CheckPhaseEnd()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// �T���v������
    /// </summary>
    void StageInstance()
    {
        sampleCreated = true;

        mapSize = stageController.MapSize; // �T�C�Y���

        // �z�� �v�f���w��
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // �����̔z�u�f�[�^���擾
        map = stageController.CorrectAnswer;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                    mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = false; // �U���I�t
                }
            }
        }
    }
}