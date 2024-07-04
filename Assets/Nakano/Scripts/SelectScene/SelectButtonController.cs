using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 選択画面のボタン処理
/// </summary>
public class SelectButtonController : MonoBehaviour
{
    [SerializeField] GameObject firstSelectPanel;
    [SerializeField, Header("10ステージ毎の選択ボタン")] private Button[] buttons_FirstSelect;
    [SerializeField, Header("ステージ数"), Tooltip("エクストラ・チュートリアルを除く")]
    private int stageAmount;

    [SerializeField] private SelectButton selectButton;
    [SerializeField, Header("各ステージの選択ボタン")] private Button[] buttons_SecondSelect;

    void Start()
    {
        for(int i = 0; i < buttons_FirstSelect.Length; i++)
        {
            buttons_FirstSelect[i].gameObject.SetActive(false);
            var fsb = buttons_FirstSelect[i].GetComponent<FirstSelectButton>();
            fsb.sbController = this;
            fsb.num = i;
        }

        // 表示するボタンの数
        int dispButton = (int)(stageAmount - 1) / 10 + 1;
        for(int i = 0; i < dispButton; i++)
        {
            buttons_FirstSelect[i].gameObject.SetActive(true);
        }

        firstSelectPanel.SetActive(true);
    }

    public void FirstSelect(int num)
    {
        firstSelectPanel.SetActive(false);

        SecondButtonsSetting(num);
    }

    void SecondButtonsSetting(int num)
    {
        for(int i = 0; i < buttons_SecondSelect.Length; i++)
        {
            string stageName = "";
            string stageName_JP = "";

            if (i == 0)
            {
                if(num == 0)
                {
                    stageName = "Tutorial";
                    stageName_JP = "チュートリアル";
                }
                else buttons_SecondSelect[i].gameObject.SetActive(false);
            }
            else if (i == buttons_SecondSelect.Length - 1)
            {
                stageName = "Extra" + (num + 1).ToString();
                stageName_JP = "エクストラステージ";
                buttons_SecondSelect[i].gameObject.SetActive(false);
            }
            else
            {
                stageName = "Stage" + (num * 10 + i).ToString();
                stageName_JP = "ステージ" + (num * 10 + i).ToString();
            }

            // Textコンポーネント取得
            var child = buttons_SecondSelect[i].transform.GetComponentInChildren<Text>();
            child.text = stageName_JP;

            var fsb = buttons_SecondSelect[i].GetComponent<SecondSelectButton>();
            fsb.selectButton = selectButton;
            fsb.stageName = stageName;
        }
    }
}
