using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionCheck : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] Image[] stars;
    [SerializeField] Sprite[] sprites;

    const int missionAmount = 3; // �~�b�V������
    bool[] missionClear;

    void Start()
    {
        missionClear = new bool[missionAmount];

        for (int i = 0; i < missionAmount; i++)
        {
            stars[i].sprite = sprites[0];
        }
    }

    void Update()
    {
    }

    /// <summary>
    /// �~�b�V�������N���A���Ă��邩�m�F
    /// </summary>
    public void Mission()
    {
        //! �~�b�V�����m�F
        StarsDisp();
    }

    /// <summary>
    /// ���̕\��
    /// </summary>
    void StarsDisp()
    {
        for(int i = 0; i < missionAmount; i++)
        {
            if(missionClear[i])
                stars[i].sprite = sprites[1];
        }
    }
}
