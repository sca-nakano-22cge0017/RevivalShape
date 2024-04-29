using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [SerializeField, Header("�X�e�[�W��")] string stageName;
    public string StageName { get { return stageName; } }

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    Vector3 mapSize = new Vector3(4, 4, 4);
    public Vector3 MapSize { get { return mapSize; } }

    ShapeData.Shape[,,] correctAnswer;
    /// <summary>
    /// ����������
    /// </summary>
    public ShapeData.Shape[,,] CorrectAnswer
    {
        get { return correctAnswer; }
        set { correctAnswer = value; }
    }

    ShapeData.Shape[,,] playerAnswer;
    /// <summary>
    /// �v���C���[�̓���
    /// </summary>
    public ShapeData.Shape[,,] PlayerAnswer
    {
        get { return playerAnswer; }
        set { playerAnswer = value; }
    }

    // ��ԍŏ��Ƀf�[�^�擾
    void Awake()
    {
        // �}�b�v�T�C�Y�擾
        mapSize = stageDataLoader.LoadStageSize(stageName);

        // �z��
        correctAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        playerAnswer = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
    }

    void Update()
    {
        
    }
}
