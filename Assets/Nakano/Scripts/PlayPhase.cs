using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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

    // ��v���v�Z
    int correctObjAmount = 0; // �����̃u���b�N��
    int matchObjAmount = 0; // �����ƈ�v���Ă���𓚃u���b�N��
    int matchRate = 0; // ��v��

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;
    bool isClear = false;

    private void Awake()
    {
        // UI���������Ă���
        objParent.gameObject.SetActive(false);
        playPhaseUI.SetActive(false);
        clearWindow.SetActive(false);

        matchRateText.enabled = false;
    }

    private void Update()
    {
        if (isClear && Input.GetMouseButton(0))
        {
            SceneManager.LoadScene("SelectScene");
        }
    }

    /// <summary>
    /// ���s�t�F�[�Y�ڍs���̏���
    /// </summary>
    public void PlayPhaseStart()
    {
        mapSize = stageController.MapSize; // �T�C�Y���

        // �z�� �v�f���w��
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // �����ƃv���C���[�̉𓚂��擾����
        correctAnswer = stageController.CorrectAnswer;
        map = stageController.PlayerAnswer;

        correctObjAmount = 0;
        matchObjAmount = 0;
        matchRate = 0;

        objParent.gameObject.SetActive(true);
        playPhaseUI.SetActive(true);

        AnswerInstance();
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

                    // �����̃u���b�N�����J�E���g
                    correctObjAmount++;

                    // �����ƈ�v���Ă���𓚃u���b�N�����J�E���g
                    if (correctAnswer[x, y, z] == map[x, y, z]) matchObjAmount++;
                }
            }
        }

        StartCoroutine(Fall());
    }

    IEnumerator Fall()
    {
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    // �󔒃}�X�Ȃ痎�����o���΂�
                    if (map[x, y, z] == ShapeData.Shape.Empty) continue;

                    mapObj[x, y, z].GetComponent<Rigidbody>().constraints =
                        RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

        MatchRateCheck();
    }

    /// <summary>
    /// �v���C���[�̉𓚂Ɛ����̈�v�����m�F�E�\��
    /// </summary>
    void MatchRateCheck()
    {
        matchRate = (int)((float)matchObjAmount / (float)correctObjAmount * 100);
        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if(matchRate >= 100)
        {
            clearWindow.SetActive(true);
            isClear = true;
        }
        else
        {
            //! �e�L�X�g�_�ŉ��o
        }
    }
}
