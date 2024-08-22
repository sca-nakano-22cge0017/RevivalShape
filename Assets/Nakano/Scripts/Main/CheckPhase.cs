using UnityEngine;

/// <summary>
/// �m�F�t�F�[�Y
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;
    [SerializeField] private MeshCombiner meshCombiner;

    // �T���v���̐e�I�u�W�F�N�g
    [SerializeField] private Transform objParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private ShapeData.Shape[,,] map; // �z�u�f�[�^
    private GameObject[,,] mapObj;   // �T���v����GameObject�^�z��

    private bool sampleCreated = false; // �T���v�������ς݂��ǂ���

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z�� �v�f���w��
        map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = ShapeData.Shape.Empty;
                    mapObj[x, y, z] = null;
                }
            }
        }
    }

    /// <summary>
    /// �m�F�t�F�[�Y�ڍs���̏���
    /// UI�\���A�T���v������
    /// </summary>
    public void PhaseStart()
    {
        checkPhaseUI.SetActive(true);
        objParent.gameObject.SetActive(true);

        // �I�u�W�F�N�g����
        SampleInstance();
    }

    public void PhaseUpdate()
    {

    }

    /// <summary>
    /// �m�F�t�F�[�Y�I��
    /// UI��\��
    /// </summary>
    public void PhaseEnd()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// �T���v������
    /// </summary>
    private void SampleInstance()
    {
        // �����ς݂Ȃ�ēx�������Ȃ�
        if (sampleCreated) return;

        // �����̔z�u�f�[�^���擾
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
                    map[x, y, z] = stageController.CorrectAnswer[x, y, z];
                }
            }
        }

        // �I�u�W�F�N�g����
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = map[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);

                    // �󔒃}�X�͐������Ȃ�
                    if (s != ShapeData.Shape.Empty && s != ShapeData.Shape.Alpha)
                    {
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, objParent);
                        mapObj[x, y, z].GetComponent<ShapeObjects>().IsVibrate = false; // �U���I�t
                    }
                }
            }
        }

        sampleCreated = true;

        // ���b�V������
        //meshCombiner.SetParent(objParent);
        //meshCombiner.Combine();
    }
}
