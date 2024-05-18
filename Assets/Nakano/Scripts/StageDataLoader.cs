using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System;

/// <summary>
/// CSV�t�@�C����ǂݍ��݁A�z�u�f�[�^�ɕϊ�����
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    // Addressable
    string stageDataText;

    [HideInInspector] public bool stageDataLoadComlete = false;

    [SerializeField] ShapeData shapeData;
    [SerializeField, Header("1�t�@�C�����̃X�e�[�W�� 10�X�e�[�W���L�q�Ȃ�10")] int StageAmountPerFile;
    [SerializeField, Header("Tutorial��Extra�̔z�u�f�[�^�������Ă���t�@�C��")] string otherStageDataFileName; 
    string stageDataFileName;

    private string selectStageName; // �I���X�e�[�W��
    
    /// <summary>
    /// �X�e�[�W�̔z�u�f�[�^�����[�h����
    /// </summary>
    /// <param name="stageName">�X�e�[�W��</param>
    public void StageDataGet(string stageName)
    {
        selectStageName = stageName;

        LoadFileSelect(stageName);

        AsyncOperationHandle<TextAsset> m_TextHandle;

        // �}�b�v�T�C�Y�̃f�[�^���擾
        Addressables.LoadAssetAsync<TextAsset>(stageDataFileName).Completed += handle => {
            m_TextHandle = handle;
            if (handle.Result == null)
            {
                Debug.Log("Load Error");
                return;
            }
            stageDataText = handle.Result.text;
            stageDataLoadComlete = true;
        };
    }

    /// <summary>
    /// �X�e�[�W�̃}�b�v�T�C�Y�f�[�^�Ǝg�p�}�`�����擾����
    /// </summary>
    /// <param name="stageName">�v���C�X�e�[�W</param>
    /// <returns>�T�C�Y��Vector3�ŁA�g�p�}�`����int�ŕԂ�</returns>
    public Vector3 LoadStageSize()
    {
        Vector3 mapSize = new Vector3(5, 5, 5);

        string dataStr = stageDataText;

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        for(int l = 0; l < line.Length; l++)
        {
            if (l + 1 > line.Length) break;

            string[] name = line[l].Split(",");
            if (name[0].ToLower() != selectStageName.ToLower()) continue;

            string[] sizeText = line[l+1].Split(","); // �X�e�[�W���̎��̍s�ɃT�C�Y���
            
            if (int.TryParse(sizeText[0], out int w)) mapSize.x = w;
            if (int.TryParse(sizeText[1], out int h)) mapSize.y = h;
            if (int.TryParse(sizeText[2], out int d)) mapSize.z = d;
            break;
        }

        return mapSize;
    }

    /// <summary>
    /// �I�u�W�F�N�g�̔z�u�f�[�^�擾
    /// </summary>
    /// <param name="stageName">�v���C�X�e�[�W</param>
    /// <returns>�O�����z��̔z�u�f�[�^��Ԃ�</returns>
    public ShapeData.Shape[,,] LoadStageMap(Vector3 mapSize)
    {
        string dataStr = stageDataText;

        // �f�[�^�z��
        ShapeData.Shape[,,] map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        int mapDataBegin = 0; // �w��X�e�[�W�̔z�u�f�[�^�̈�ԍŏ��̃f�[�^�̈ʒu

        for (int l = 0; l < line.Length; l++)
        {
            string[] name = line[l].Split(",");
            if (name[0].ToLower() != selectStageName.ToLower()) continue;

            mapDataBegin = l + 2;

            break;
        }

        for (int z = mapDataBegin; z < mapSize.z + mapDataBegin; z++)
        {
            // �X�L�b�v���������́i��������⑫�j�ɗ������΂�
            if (line[z].StartsWith("!")) continue;

            // MapSizeData�Ŏw�肵��Depth���f�[�^�������Ȃ������ꍇ�͕⊮
            string[] l = DataComplement((int)mapSize.z, line.Length, line, z);

            var cell = l[z].Split(","); // �R���}�ŋ�؂� �Z�����Ƃɕ������

            for (int x = 0; x < mapSize.x; x++)
            {
                // MapSizeData�Ŏw�肵��Width���f�[�^�������Ȃ������ꍇ�͕⊮
                string[] c = DataComplement((int)mapSize.x, cell.Length, cell, x);

                var obj = c[x].Split("/"); // �X���b�V���ŋ�؂�

                for (int y = 0; y < mapSize.y; y++)
                {
                    // MapSizeData�Ŏw�肵��Height���f�[�^�������Ȃ������ꍇ�͕⊮
                    string[] o = DataComplement((int)mapSize.y, obj.Length, obj, y);

                    map[x, y, z - mapDataBegin] = shapeData.StringToShape(o[y]);
                }
            }
        }

        return map;
    }

    /// <summary>
    /// �f�[�^�⊮
    /// </summary>
    /// <param name="idealDataSize">MapSizeData�Ŏw�肵���f�[�^���@mapSize</param>
    /// <param name="actualDataSize">���ۂɂ���f�[�^���@string[]�̗v�f��</param>
    /// <param name="data">�z�u�f�[�^�������Ă���string[]</param>
    /// <param name="dataNum">�z��̉��Ԗڂ�</param>
    /// <returns></returns>
    string[] DataComplement(int idealDataSize, int actualDataSize, string[] data, int dataNum) 
    {
        // MapSizeData�Ŏw�肵���f�[�^�����A���ۂ̃f�[�^�������Ȃ������ꍇ�͕⊮
        int n = idealDataSize > actualDataSize ? idealDataSize : actualDataSize;
        string[] s = new string[n];
        s[dataNum] = actualDataSize <= dataNum ? s[dataNum] = "" : data[dataNum];
        return s;
    }

    /// <summary>
    /// �X�e�[�W���ɉ����ēǂݍ��ރt�@�C����ς���
    /// </summary>
    /// <param name="stageName">�X�e�[�W��</param>
    void LoadFileSelect(string stageName)
    {
        if(stageName.Contains("Stage"))
        {
            stageName = stageName.Replace("Stage", "");

            if (int.TryParse(stageName, out int n))
            {
                // �X�e�[�W�̔ԍ��ɉ����ăt�@�C�����擾
                stageDataFileName = ((n - 1) / StageAmountPerFile + 1).ToString();

                return;
            }

            else
            {
                stageDataFileName = otherStageDataFileName;
                return;
            }
        }

        else
        {
            // �X�e�[�W�����ԍ�����Ȃ��ꍇ�iTutorial��ExtraStage�̏ꍇ�j�͎w��t�@�C����ǂݍ���
            stageDataFileName = otherStageDataFileName;
            return;
        }
    }
}