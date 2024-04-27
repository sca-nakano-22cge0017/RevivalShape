using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class StageDataLoader : MonoBehaviour
{
    string folderName = "/Nakano/StageData";

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

    public List<List<List<StageCreate.Shape>>> LoadStage(string stageName)
    {
        string fileName = Application.dataPath + folderName + "/" + stageName + ".csv";
        string dataStr = "";

        FileCheck(fileName); // ���݊m�F

        Vector3 size = new Vector3(0,0,0);
        List<List<List<StageCreate.Shape>>> map = new();

        StreamReader reader = new StreamReader(fileName);
        dataStr = reader.ReadToEnd();
        reader.Close();

        string[] line = dataStr.Split("\n"); // ���s�ŕ���

        for(int z = 0; z < line.Length; z++)
        {
            // �X�L�b�v���������́i��������⑫�j�ɗ�����I���ɂ���
            if (line[0][0] == '!') break;

            var cell = line[z].Split(","); // �R���}�ŋ�؂� �Z�����Ƃɕ������
            
            if(z == 0) // 1�s�ڂɏ����Ă��镝�E���s�E��������
            {
                if (int.TryParse(cell[0], out int w)) size.x = w;
                if (int.TryParse(cell[1], out int d)) size.z = d;
                if (int.TryParse(cell[2], out int h)) size.y = h;
            }

            else
            {
                // 3�����̃��X�g�ɑ��
                List<List<StageCreate.Shape>> zLine = new();

                for (int x = 0; x < cell.Length; x++)
                {
                    var obj = cell[x].Split("/"); // �X���b�V���ŋ�؂�

                    List<StageCreate.Shape> xCell = new();

                    for (int y = 0; y < size.z; y++)
                    {
                        // �z�񂩂�f�[�^���R��Ȃ��悤�ɒ�������
                        int h = (int)size.z > obj.Length ? (int)size.z : obj.Length;
                        string[] o = new string[h];
                        
                        // �w��̃f�[�^���ɑ���Ȃ�������f�[�^����ǉ��␳
                        if(obj.Length <= y) { o[y] = ""; }
                        else o[y] = obj[y];

                        // ��������}�`����
                        StageCreate.Shape s = StringToShape(o[y]);
                        xCell.Add(s);
                    }

                    zLine.Add(xCell);
                }

                map.Add(zLine);
            }
        }

        return map;
    }

    StageCreate.Shape StringToShape(string s)
    {
        StageCreate.Shape shape = new();

        switch (s)
        {
            case "C":
            case "c":
                shape = StageCreate.Shape.Cube;
                break;

            case "S":
            case "s":
                shape = StageCreate.Shape.Sphere;
                break;

            case "E":
            case "e":
            case "":
                shape = StageCreate.Shape.Empty;
                break;
        }

        return shape;
    }
}
