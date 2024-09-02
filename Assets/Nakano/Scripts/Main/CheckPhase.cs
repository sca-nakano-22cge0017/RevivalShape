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

    [SerializeField, Header("1���琶�����邩")] private bool isCreate = false;

    [SerializeField, Header("�����ꏊ")] private Transform createParent;
    [SerializeField, Header("�T���v���̐e�I�u�W�F�N�g")] private Transform sampleParent;
    [SerializeField, Header("�e�X�e�[�W�̃T���v��")] private GameObject[] samples;
    [SerializeField, Header("�e�X�e�[�W�̃T���v�� Tutorial��Extra")] private GameObject[] samplesOther;

    [SerializeField] private GameObject checkPhaseUI;

    private Vector3 mapSize;

    private GameObject[,,] mapObj;   // �T���v����GameObject�^�z��

    private bool sampleCreated = false; // �T���v�������ς݂��ǂ���

    public void Initialize()
    {
        checkPhaseUI.SetActive(false);
        sampleParent.gameObject.SetActive(false);

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
        sampleParent.gameObject.SetActive(true);

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
        sampleParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// �T���v������
    /// </summary>
    private void SampleInstance()
    {
        if(!isCreate)
        {
            SampleDisplay();
            return;
        }

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
                        mapObj[x, y, z] = Instantiate(obj, pos, Quaternion.identity, createParent);
                    }
                }
            }
        }

        sampleCreated = true;
    }

    /// <summary>
    /// �����E�����ς݃T���v����\��
    /// </summary>
    private void SampleDisplay()
    {
        string stageName = stageController.StageName;

        if (stageName.Contains("Stage"))
        {
            string _stageName = stageName.Replace("Stage", "");

            if (int.TryParse(_stageName, out int n))
            {
                if (n - 1 >= 0 && n - 1 < samples.Length)
                {
                    if (!SampleNullCheck(samples[n - 1])) samples[n - 1].SetActive(true);
                }

                return;
            }
        }

        else if (stageName.Contains("Tutorial"))
        {
            if (!SampleNullCheck(samplesOther[0])) samplesOther[0].SetActive(true);
        }

        else if (stageName.Contains("Extra"))
        {
            string _stageName = stageName.Replace("Extra", "");

            if (int.TryParse(_stageName, out int n))
            {
                if (!SampleNullCheck(samplesOther[n])) samplesOther[n].SetActive(true);
            }
        }
    }

    bool SampleNullCheck(GameObject _object)
    {
        bool isNull = false;

        if (_object == null)
        {
            isNull = true;
            isCreate = true;
            SampleInstance();
        }

        return isNull;
    }
}
