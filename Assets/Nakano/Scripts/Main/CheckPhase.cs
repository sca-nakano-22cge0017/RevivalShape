using UnityEngine;

[System.Serializable]
public class CombineTests
{
    public ShapeData.Shape shape;
    public CombineTest combineTest;
    public Transform parent;        // parent�̎q�I�u�W�F�N�g��S�Č�������
    public GameObject combinedObj;  // ������̃I�u�W�F�N�g
}

/// <summary>
/// �m�F�t�F�[�Y
/// </summary>
public class CheckPhase : MonoBehaviour, IPhase
{
    [SerializeField] private ShapeData shapeData;
    [SerializeField] private StageController stageController;
    [SerializeField] private MeshCombiner meshCombiner;
    [SerializeField] private CombineTests[] combineTests;
    [SerializeField, Header("�������邩")] private bool isCombine = false;

    // �T���v���̐e�I�u�W�F�N�g
    [SerializeField] private Transform objParent;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // �T���v����GameObject�^�z��

    private bool sampleCreated = false; // �T���v�������ς݂��ǂ���

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        objParent.gameObject.SetActive(false);

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

                    // �󔒃}�X�͐������Ȃ�
                    if (s != ShapeData.Shape.Empty)
                    {
                        Transform parent = GetParent(s);
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, parent);
                    }
                }
            }
        }

        sampleCreated = true;

        if(isCombine) Combine();
    }

    // ���b�V������
    void Combine()
    {
        for (int i = 0; i < combineTests.Length; i++)
        {
            if (combineTests[i].shape != ShapeData.Shape.Empty)
            {
                Transform parent = combineTests[i].parent;
                CombineTest ct = combineTests[i].combineTest;
                ct.Combine(stageController.StageName, combineTests[i].shape, parent);
            }
        }
    }

    Transform GetParent(ShapeData.Shape _shape)
    {
        Transform objParent = null;

        for (int i = 0; i < combineTests.Length; i++)
        {
            if (combineTests[i].shape == _shape)
            {
                objParent = combineTests[i].parent;
            }
        }

        return objParent;
    }

    /// <summary>
    /// ���b�V��������̃I�u�W�F�N�g���擾
    /// </summary>
    void GetCombinedObject()
    {

    }
}
