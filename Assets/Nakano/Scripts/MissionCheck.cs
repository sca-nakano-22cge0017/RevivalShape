using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionCheck : MonoBehaviour
{
    [SerializeField] StageController stageController;

    [SerializeField] Image[] stars;
    [SerializeField] Sprite[] sprites;

    const int missionAmount = 3; // ミッション数
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
    /// ミッションをクリアしているか確認
    /// </summary>
    public void Mission()
    {
        //! ミッション確認
        StarsDisp();
    }

    /// <summary>
    /// ☆の表示
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
