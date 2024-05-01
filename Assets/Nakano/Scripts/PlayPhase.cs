using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;

    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    ShapeData.Shape[,,] correctAnswer; // ����

    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ�����")] int fallPos;

    void Start()
    {
        objParent.gameObject.SetActive(false);
        playPhaseUI.SetActive(false);

        mapSize = stageController.MapSize; // �T�C�Y���

        // �z�� �v�f���w��
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
    }

    void Update()
    {
    }

    void AnswerInstance()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                }
            }
        }

        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    if(map[x, y, z] == ShapeData.Shape.Empty) break;

                    mapObj[x, y, z].GetComponent<Rigidbody>().constraints = 
                        RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }
    }

    public void PlayPhaseStart()
    {
        // �����ƃv���C���[�̉𓚂��擾����
        correctAnswer = stageController.CorrectAnswer;
        map = stageController.PlayerAnswer;

        objParent.gameObject.SetActive(true);
        playPhaseUI.SetActive(true);

        AnswerInstance();
    }
}
