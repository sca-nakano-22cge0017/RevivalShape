using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

/// <summary>
/// CSV�t�@�C����ǂݍ��݁A�z�u�f�[�^�ɕϊ�����
/// </summary>
public class StageDataLoader : MonoBehaviour
{
    [SerializeField, Header("�f�[�^���s�����Ă���ꍇ�G���[��\�����܂���")] bool isErrorDisp = false;
    [SerializeField] ShapeData shapeData;

    string folderName = "/Nakano/StageData";
    string mapSizeFile = "/MapSizeData.csv";

    /// <summary>
    /// �t�@�C���E�f�B���N�g���̑��݊m�F
    /// </summary>
    /// <param name="fileName">�t�@�C����</param>
    /// <param name="length">�����L���O�f�[�^�̔z��</param>
    void FileCheck(string name)
    {
        string directoryName = Application.dataPath + folderName;
        string fileName = name;

        while (!Directory.Exists(directoryName)) //�f�B���N�g�����Ȃ�������
        {
            Directory.CreateDirectory(directoryName); //�f�B���N�g�����쐬
        }

        while (!File.Exists(fileName)) // �t�@�C�����Ȃ�������
        {
            FileStream fs = File.Create(fileName); // �t�@�C�����쐬
            fs.Close(); // �t�@�C�������
        }
    }

    /// <summary>
    /// �I�u�W�F�N�g�̔z�u�f�[�^�擾
    /// </summary>
    /// <param name="stageName">�v���C�X�e�[�W</param>
    /// <returns>�O�����̔z�u�f�[�^��Ԃ�</returns>
    public ShapeData.Shape[,,] LoadStageMap(string stageName)
    {
        string fileName = Application.dataPath + folderName + "/" + stageName + ".csv";
        string dataStr = "";

        FileCheck(fileName); // ���݊m�F

        // �}�b�v�̃T�C�Y���擾
        Vector3 mapSize = LoadStageSize(stageName);

        // �f�[�^�z��
        ShapeData.Shape[,,] map = new ShapeData.Shape[(int)mapSize.x, (int)mapSize.y, (int)mapSize.z];

        // �ǂݍ���
        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        for (int z = 0; z < mapSize.z; z++)
        {
            // �X�L�b�v���������́i��������⑫�j�ɗ�����I���ɂ���
            if (line[z][0] == '!') break;

            // �G���[�\��
            if ((int)mapSize.y > line.Length && isErrorDisp)
            {
                Debug.LogWarning("Z������(Excel�c����)�̃f�[�^���s�����Ă��܂��B�󔒃}�X�ŕ⊮���܂��B �ΏۃZ���F�c" + (z + 1) + "�Ԗ�");
            }

            // MapSizeData�Ŏw�肵��Depth���f�[�^�������Ȃ������ꍇ�͕⊮
            int d = (int)mapSize.z > line.Length ? (int)mapSize.z : line.Length;
            string[] l = new string[d];
            l[z] = line.Length <= z ? l[z] = "" : line[z];

            var cell = l[z].Split(","); // �R���}�ŋ�؂� �Z�����Ƃɕ������

            for (int x = 0; x < mapSize.x; x++)
            {
                // �G���[�\��
                if ((int)mapSize.x > cell.Length && isErrorDisp)
                {
                    Debug.LogWarning("X�������iExcel�������j�̃f�[�^���s�����Ă��܂��B�󔒃}�X�ŕ⊮���܂��B �Ώۗ�F��" + (x + 1) + "�Ԗ�");
                }
                // MapSizeData�Ŏw�肵��Width���f�[�^�������Ȃ������ꍇ�͕⊮
                int w = (int)mapSize.x > cell.Length ? (int)mapSize.x : cell.Length;
                string[] c = new string[w];
                c[x] = cell.Length <= x ? c[x] = "": cell[x];

                var obj = c[x].Split("/"); // �X���b�V���ŋ�؂�

                for (int y = 0; y < mapSize.y; y++)
                {
                    // �G���[�\��
                    if((int)mapSize.y > obj.Length && isErrorDisp)
                    {
                        Debug.LogWarning("Y�������̃f�[�^���s�����Ă��܂��B�󔒃}�X�ŕ⊮���܂��B �ΏۃZ���F��" + (x+1) + "�Ԗ�,�c" + (z+1) + "�Ԗ�");
                    }

                    // MapSizeData�Ŏw�肵��Height���f�[�^�������Ȃ������ꍇ�͕⊮
                    int h = (int)mapSize.y > obj.Length ? (int)mapSize.y : obj.Length;
                    string[] o = new string[h];
                    o[y] = obj.Length <= y ? o[y] = "": obj[y];

                    map[x, y, z] = shapeData.StringToShape(o[y]);
                }
            }
        }

        return map;
    }

    /// <summary>
    /// �X�e�[�W�̃}�b�v�T�C�Y�f�[�^���擾����
    /// </summary>
    /// <param name="stageName">�v���C�X�e�[�W</param>
    /// <returns>�T�C�Y��Vector3�ŕԂ�</returns>
    public Vector3 LoadStageSize(string stageName)
    {
        Vector3 mapSize = new Vector3(0, 0, 0);

        string fileName = Application.dataPath + folderName + mapSizeFile;
        string dataStr = "";

        FileCheck(fileName); // ���݊m�F

        // �ǂݍ���
        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        for (int z = 0; z < line.Length; z++)
        {
            if(z == 0) continue; // �X�L�b�v���������́i��������⑫�j�ɗ����� / 1�s�ڂ��΂�
            if(line[z][0] == '!' || line[z][0] == '�I') break;

            var cell = line[z].Split(","); // �R���}�ŋ�؂� �Z�����Ƃɕ������

            // �X�e�[�W���Ɠ����ɂȂ�܂ł�蒼�� �啶������������ʂ��Ȃ�
            if(string.Compare(stageName, cell[0], true) != 0) continue;

            // �}�b�v�̃T�C�Y����
            if (int.TryParse(cell[1], out int w)) mapSize.x = w;
            if (int.TryParse(cell[2], out int d)) mapSize.z = d;
            if (int.TryParse(cell[3], out int h)) mapSize.y = h;
        }

        return mapSize;
    }
}