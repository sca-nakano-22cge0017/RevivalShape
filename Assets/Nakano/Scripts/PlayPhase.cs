using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayPhase : MonoBehaviour
{
    [SerializeField] StageController stageController;
    [SerializeField] ShapeData shapeData;
    [SerializeField] MissionCheck missionCheck;

    [SerializeField] Transform objParent;

    [SerializeField] GameObject playPhaseUI;

    Vector3 mapSize;

    ShapeData.Shape[,,] map;
    GameObject[,,] mapObj;

    ShapeData.Shape[,,] correctAnswer; // ����

    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ�����")] int fallPos;
    [SerializeField, Header("�I�u�W�F�N�g�𗎂Ƃ��Ԋu(sec)")] float fallInterval;
    [SerializeField, Tooltip("�I�u�W�F�N�g���S�ė������Ă����v���\���܂ł̎���(sec)")]
    float fallToMatchdispTime;

    // ��v���v�Z
    float matchRateTroutTotal = 0; // 1�}�X������̈�v���̘a
    int hasBlockTrout = 0; // 1�ȏ�u���b�N������}�X�̐�
    int c_hasBlockTrout = 0; // ������1�ȏ�u���b�N������}�X�̐�
    int matchRate = 0; // ��v��

    [SerializeField] Text matchRateText;

    [SerializeField] GameObject clearWindow;
    bool isClear = false;
    bool isRetry = false;

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

        if (isRetry && Input.GetMouseButton(0))
        {
            // �m�F�t�F�[�Y�ɖ߂�
            stageController.ToCheckPhase();
            PlayPhaseEnd();
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

        // ��v�� ������
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        c_hasBlockTrout = 0;
        matchRate = 0;

        objParent.gameObject.SetActive(true);
        playPhaseUI.SetActive(true);

        AnswerInstance();
    }

    void AnswerInstance()
    {
        matchRateTroutTotal = 0;
        hasBlockTrout = 0;
        c_hasBlockTrout = 0;
        matchRate = 0;

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                float matchRateTrout = 0; // 1�}�X���̈�v��

                int total = 0; // 1�}�X���̃u���b�N�̑���
                int c_total = 0; // ������1�}�X���̃u���b�N�̑���
                int excess = 0; // ���ߐ�
                int lack = 0; // �s����
                int diff = 0; // ���ᐔ

                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y + fallPos, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);

                    if(map[x, y, z] != ShapeData.Shape.Empty) total++;
                    if(correctAnswer[x, y, z] != ShapeData.Shape.Empty) c_total++;

                    // ���ߕ� �����ł͉����u����Ă��Ȃ��ꏊ�ɃI�u�W�F�N�g���u���ꂽ�ꍇ
                    if(correctAnswer[x, y, z] == ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        excess++;
                    }

                    // �s���� �����ł͉����u����Ă���ꏊ�ɃI�u�W�F�N�g�������u����Ă��Ȃ������ꍇ
                    else if (correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] == ShapeData.Shape.Empty)
                    {
                        lack++;
                    }

                    // ���ᕪ �`���Ⴄ�ꍇ
                    else if(correctAnswer[x, y, z] != map[x, y, z] && correctAnswer[x, y, z] != ShapeData.Shape.Empty && map[x, y, z] != ShapeData.Shape.Empty)
                    {
                        diff++;
                    }
                }

                if(total > 0) hasBlockTrout++;
                if(c_total > 0) c_hasBlockTrout++;

                // 1�}�X������̈�v�����v�Z
                // ���߂�����ꍇ
                if(excess > 0)
                {
                    matchRateTrout = 1 - ((total - (c_total - diff)) / total);
                    // 1 - (( 1�}�X���̑��u���b�N�� ) - (( ������1�}�X���̃u���b�N�� ) - ���ᐔ ) / 1�}�X���̑��u���b�N�� )
                }
                else
                {
                    if(c_total > 0)
                        matchRateTrout = (c_total - lack - diff) / c_total;
                    // (( ������1�}�X���̃u���b�N�� ) - ( �s���� ) - ( ���ᕪ )) / ( ������1�}�X���̃u���b�N�� )

                    // �����u���ĂȂ��ꍇ��0
                    if (c_total <= 0)
                        matchRateTrout = 0;
                }

                matchRateTroutTotal += matchRateTrout;
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
                    yield return new WaitForSeconds(fallInterval);
                }
            }
        }

        yield return new WaitForSeconds(fallToMatchdispTime);
        MatchRateDisp();
    }

    /// <summary>
    /// �v���C���[�̉𓚂Ɛ����̈�v�����m�F�E�\��
    /// </summary>
    void MatchRateDisp()
    {
        int hBT = hasBlockTrout > c_hasBlockTrout ? hasBlockTrout : c_hasBlockTrout;
        matchRate = (int)(matchRateTroutTotal / (float)hBT * 100);

        matchRateText.enabled = true;
        matchRateText.text = matchRate.ToString() + "%";

        if(matchRate >= 100)
        {
            clearWindow.SetActive(true);
            missionCheck.Mission();
            isClear = true;
        }
        else
        {
            StartCoroutine(MatchTextBlinking());
            isRetry = true;
        }
    }

    float unDispTime = 0.3f;
    float dispTime = 0.5f;

    /// <summary>
    /// �p�[�Z���e�[�W�_�ŉ��o
    /// </summary>
    IEnumerator MatchTextBlinking()
    {
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForSeconds(dispTime);
            matchRateText.enabled = false;
            yield return new WaitForSeconds(unDispTime);
            matchRateText.enabled = true;
        }
    }

    public void PlayPhaseEnd()
    {
        // �E�B���h�E�����
        playPhaseUI.SetActive(false);
    }
}
