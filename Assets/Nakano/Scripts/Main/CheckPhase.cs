using UnityEngine;

/// <summary>
/// �m�F�t�F�[�Y
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;

    [SerializeField] private Combiners[] combiners;

    [SerializeField, Header("�����ꏊ")] private Transform samplesParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // �T���v����GameObject�^�z��

    private bool sampleCreated = false; // �T���v�������ς݂��ǂ���

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        samplesParent.gameObject.SetActive(false);

        // �}�b�v�T�C�Y�擾
        mapSize = stageController.MapSize;

        // �z�� �v�f���w��
        mapObj = new GameObject[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        for (int z = 0; z < mapSize.z; z++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                for (int x = 0; x < mapSize.x; x++)
                {
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
        samplesParent.gameObject.SetActive(true);

        UsedBlocksCheck();

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
        samplesParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// �T���v������
    /// </summary>
    private void SampleInstance()
    {
        // �����ς݂Ȃ�ēx�������Ȃ�
        if (sampleCreated) return;

        // �I�u�W�F�N�g����
        for (int z = 0; z < mapSize.z; z++)
        {
            for (int x = 0; x < mapSize.x; x++)
            {
                for (int y = 0; y < mapSize.y; y++)
                {
                    Vector3 pos = new Vector3(-x, y, z);

                    ShapeData.Shape s = stageController.CorrectAnswer[x, y, z];
                    GameObject obj = shapeData.ShapeToPrefabs(s);
                    Transform parent = GetCreateParent(s);
                    
                    // �󔒃}�X�͐������Ȃ�
                    if (s != ShapeData.Shape.Empty)
                    {
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, parent);
                    }
                }
            }
        }

        sampleCreated = true;

        MeshCombine();
    }

    void UsedBlocksCheck()
    {
        for (int s = 0; s < stageController.ShapeType.Length; s++)
        {
            for (int c = 0; c < combiners.Length; c++)
            {
                if(combiners[c].shape == stageController.ShapeType[s])
                {
                    combiners[c].isShapeUsed = true;
                }
            }
        }
    }

    void MeshCombine()
    {
        for (int i = 0; i < combiners.Length; i++)
        {
            if (combiners[i].isShapeUsed)
            {
                combiners[i].meshCombiner.SetParent(combiners[i].parent);
                combiners[i].meshCombiner.Combine(true, false);
            }
        }
    }

    /// <summary>
    /// �����u���b�N�ɉ����Đ�������e�I�u�W�F�N�g��Ԃ�
    /// </summary>
    /// <param name="_shape"></param>
    Transform GetCreateParent(ShapeData.Shape _shape)
    {
        Transform parent = null;

        for (int i = 0; i < combiners.Length; i++)
        {
            if (combiners[i].shape == _shape)
            {
                parent = combiners[i].parent;
                break;
            }
        }

        return parent;
    }
}
