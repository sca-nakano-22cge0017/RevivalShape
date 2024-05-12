using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// CSV�t�@�C����ǂݍ��݁A�z�u�f�[�^�ɕϊ�����
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    // Addressable
    string stageDataText;

    [HideInInspector] public bool stageDataLoadComlete = false;

    [SerializeField] ShapeData shapeData;

    /// <summary>
    /// �X�e�[�W�̔z�u�f�[�^�����[�h����
    /// </summary>
    /// <param name="stageName">�X�e�[�W��</param>
    public void StageDataGet(string stageName)
    {
        AsyncOperationHandle<TextAsset> m_TextHandle;

        // �}�b�v�T�C�Y�̃f�[�^���擾
        Addressables.LoadAssetAsync<TextAsset>(stageName).Completed += handle => {
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

        for (int z = 1; z <= mapSize.z; z++)
        {
            // �X�L�b�v���������́i��������⑫�j�ɗ�����I���ɂ���
            if (line[z].StartsWith("!")) break;

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

                    map[x, y, z - 1] = shapeData.StringToShape(o[y]);
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
    /// �X�e�[�W�̃}�b�v�T�C�Y�f�[�^���擾����
    /// </summary>
    /// <param name="stageName">�v���C�X�e�[�W</param>
    /// <returns>�T�C�Y��Vector3�ŕԂ�</returns>
    public Vector3 LoadStageSize(string stageName)
    {
        Vector3 mapSize = new Vector3(0, 0, 0);

        string dataStr = stageDataText;

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        string[] sizeText = line[0].Split(",");
        if (int.TryParse(sizeText[1], out int w)) mapSize.x = w;
        if (int.TryParse(sizeText[2], out int d)) mapSize.z = d;
        if (int.TryParse(sizeText[3], out int h)) mapSize.y = h;

        return mapSize;
    }
}