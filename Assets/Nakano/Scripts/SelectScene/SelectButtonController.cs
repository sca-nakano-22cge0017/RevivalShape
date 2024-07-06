using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �I����ʂ̃{�^������ UI�\��
/// </summary>
public class SelectButtonController : MonoBehaviour
{
    GameManager gameManager = null;
    private int stageAmount = 0;

    Dictionary<int, bool> stageRelease = new(); // ������
    Dictionary<int, bool> extraRelease = new();

    [SerializeField] GameObject firstSelectPanel;
    [SerializeField, Header("10�X�e�[�W���̑I���{�^��")] private Button[] buttons_FirstSelect;
    
    [SerializeField] private SelectButton selectButton;
    [SerializeField, Header("�e�X�e�[�W�̑I���{�^��")] private Button[] buttons_SecondSelect;
    [SerializeField] private Sprite[] missionIcons_sp;
    [SerializeField, Header("Content")] private RectTransform secondContent = null;

    [SerializeField, Header("�G�L�X�g���{�^��������������Content�̏c��")] private float contentHeight = 400.0f;
    [SerializeField] private int width;
    [SerializeField] private int maxHeight;

    bool loaded = false;

    void Start()
    {
        if (GameObject.FindObjectOfType<GameManager>() != null)
        {
            gameManager = GameObject.FindObjectOfType<GameManager>();
            stageAmount = gameManager.StageAmount;
        }

        for (int i = 0; i < stageAmount / 10; i++)
        {
            if (i == 0) stageRelease.Add(i, true);
            else stageRelease.Add(i, false);
            extraRelease.Add(i, false);
        }

        loaded = false;
    }

    private void Update()
    {
        if(gameManager != null)
        {
            if (gameManager.DidLoad && !loaded)
            {
                loaded = true;
                FirstButtonsSetting();
            }
        }
    }

    void FirstButtonsSetting()
    {
        firstSelectPanel.SetActive(true);

        for (int i = 0; i < stageAmount / 10; i++)
        {
            var fsb = buttons_FirstSelect[i].GetComponent<FirstSelectButton>();
            fsb.sbController = this;
            fsb.num = i;

            Release(i);
            buttons_FirstSelect[i].interactable = stageRelease[i];
        }
    }

    public void FirstSelect(int num)
    {
        firstSelectPanel.SetActive(false);

        SecondButtonsSetting(num);
    }

    void SecondButtonsSetting(int num)
    {
        int undispButton = 0;
        for (int i = 0; i < buttons_SecondSelect.Length; i++)
        {
            string stageName = "";
            
            if (i == 0)
            {
                stageName = "Tutorial";
                if (num != 0)
                {
                    buttons_SecondSelect[i].gameObject.SetActive(false);
                    undispButton++;
                }
            }
            else if (i == buttons_SecondSelect.Length - 1)
            {
                stageName = "Extra" + (num + 1).ToString();
                buttons_SecondSelect[i].interactable = extraRelease[num];

                // ���łł�11�`20�X�e�[�W��Extra�͖���
                if(num == 1)
                {
                    buttons_SecondSelect[i].gameObject.SetActive(false);
                    undispButton++;
                }
            }
            else
            {
                stageName = "Stage" + (num * 10 + i).ToString();
            }

            // Text�\���ύX
            var childText = buttons_SecondSelect[i].transform.GetComponentInChildren<Text>();
            childText.text = stageName.ToUpper();

            // Extra�ATutorial�ȊO�̓~�b�V�����N���A�̃A�C�R���\����ύX
            if(stageName.Contains("Stage"))
            {
                if (gameManager.GetStageData(stageName) != null)
                {
                    StageData data = gameManager.GetStageData(stageName);
                    var imageParent = buttons_SecondSelect[i].transform.Find("Mission");
                    MissionIconDisp(imageParent, data);
                }
            }
            
            Debug.Log(stageName);
            var fsb = buttons_SecondSelect[i].GetComponent<SecondSelectButton>();
            fsb.selectButton = selectButton;
            fsb.stageName = stageName;
        }

        // Content�T�C�Y����
        var height = maxHeight - (undispButton * contentHeight);
        secondContent.sizeDelta = new Vector2(width, height);
    }

    void MissionIconDisp(Transform _parent, StageData _data)
    {
        for (int i = 0; i < 3; i++)
        {
            int spNum = _data.IsMissionClear[i] ? 1 : 0;
            _parent.GetChild(i).GetComponent<Image>().sprite = missionIcons_sp[spNum];
        }
    }

    void Release(int num)
    {
        int starsAmount = 0;

        for(int i = 1; i <= 10; i++)
        {
            string stageName = "Stage" + (i + num).ToString();

            if(gameManager.GetStageData(stageName) != null)
                starsAmount += gameManager.GetStageData(stageName).GotStar;
        }

        if (starsAmount >= 25)
        {
            if(num < stageAmount) stageRelease[num + 1] = true;
        }
        if (starsAmount >= 30)
        {
            extraRelease[num] = true;
        }
    }
}
