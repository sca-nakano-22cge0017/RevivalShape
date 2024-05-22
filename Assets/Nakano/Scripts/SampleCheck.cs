using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// �T���v�������m�F�p
/// </summary>
public class SampleCheck : MonoBehaviour
{
    [SerializeField, Header("�X�e�[�W��")] string stageName;

    [SerializeField] ShapeData shapeData;
    [SerializeField] StageDataLoader stageDataLoader;

    [SerializeField] CameraRotate cameraRotate;
    [SerializeField] SheatCreate sheatCreate;

    // �T���v���̐e�I�u�W�F�N�g
    [SerializeField] Transform objParent;

    public Vector3 MapSize { get; private set; } = new Vector3(4, 4, 4);

    // �f�[�^�擾�����������ǂ���
    bool dataGot = false;

    // �z�u�f�[�^
    public ShapeData.Shape[,,] correctAnswer;

    // �T���v�������ς݂��ǂ���
    bool sampleCreated = false;

    void Awake()
    {
        stageDataLoader.StageDataGet(stageName);  // �X�e�[�W�̔z�u�f�[�^�����[�h�J�n
    }

    void Update()
    {
        // ���[�h���I����Ă��Ȃ���Ύ��̏����ɐi�܂��Ȃ�
        if (!stageDataLoader.stageDataLoadComlete) return;

        // �f�[�^��ϐ��Ƃ��Ď擾���Ă��Ȃ����
        if (!dataGot)
        {
            dataGot = true;

            // �}�b�v�T�C�Y�擾
            MapSize = stageDataLoader.LoadStageSize();

            // �z�� �v�f���w��
            correctAnswer = new ShapeData.Shape[(int)MapSize.x, (int)MapSize.y, (int)MapSize.z];

            cameraRotate.TargetSet();
            cameraRotate.CanRotate = true;

            // �����擾
            correctAnswer = stageDataLoader.LoadStageMap(MapSize);

            // �V�[�g�쐬
            sheatCreate.Sheat();

            StageInstance();
        }
    }
    void StageInstance()
    {
        // �����ς݂Ȃ�ēx�������Ȃ�
        if (sampleCreated) return;

        // ����
        for (int z = 0; z < MapSize.z; z++)
        {
            for (int x = 0; x < MapSize.x; x++)
            {
                for (int y = 0; y < MapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = correctAnswer[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    Instantiate(obj, pos, Quaternion.identity, objParent);
                }
            }
        }

        sampleCreated = true;
    }

}
