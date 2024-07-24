using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class Windows
{
    public GameObject order;
    public GameObject[] objects;
}

/// <summary>
/// チュートリアル制御
/// </summary>
public class Tutorial : MonoBehaviour
{
    [SerializeField] private StageController stageController;

    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private Windows[] checkPhase;
    [SerializeField] private Windows[] selectPhase;
    [SerializeField] private Windows[] playPhase;

    private int tapCount = 0;

    delegate void PlayFunc();
    PlayFunc playFunc;

    private string methodName = "";
    public string MethodName { get { return methodName; } private set { methodName = value; } }

    private void OnEnable()
    {
        tutorialCanvas.SetActive(true);

        playFunc = CheckA;
    }

    void Update()
    {
        if(playFunc != null)
        {
            playFunc();

            methodName = playFunc.Method.Name;
        }
    }

    void TentativeFunc()
    {

    }

    void CheckA()
    {
        if(!checkPhase[0].order.activeSelf)
        {
            checkPhase[0].order.SetActive(true);
        }

        if(Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapCount++;

            for (int i = 0; i < checkPhase[0].objects.Length; i++)
            {
                if (i == tapCount)
                {
                    checkPhase[0].objects[i].SetActive(true);
                }
                else checkPhase[0].objects[i].SetActive(false);
            }

            if(tapCount >= checkPhase[0].objects.Length)
            {
                checkPhase[0].order.SetActive(false);
                tapCount = 0;
                playFunc = CheckB;
            }
        }
    }

    private bool toCheckB_2 = false;
    public bool ToCheckB_2 { get { return toCheckB_2;} set { toCheckB_2 = value; } }

    private bool toCheckC = false;
    public bool ToCheckC { get { return toCheckC; } set { toCheckC = value; } }
    void CheckB()
    {
        if (!checkPhase[1].order.activeSelf)
        {
            checkPhase[1].order.SetActive(true);
            checkPhase[1].objects[0].SetActive(true);
        }

        if(toCheckB_2 && !checkPhase[1].objects[1].activeSelf)
        {
            checkPhase[1].objects[0].SetActive(false);
            checkPhase[1].objects[1].SetActive(true);
        }

        if(toCheckC)
        {
            checkPhase[1].order.SetActive(false);
            checkPhase[1].objects[1].SetActive(false);
            playFunc = CheckC;
        }
    }

    private bool isCheckC = false;
    public bool IsCheckC { get { return isCheckC; } set { isCheckC = value; } }
    private bool toCheckD = false;
    public bool ToCheckD { get { return toCheckD; } set { toCheckD = value; } }

    public void GoToCheckD()
    {
        isCheckC = true;
    }

    void CheckC()
    {
        if (!checkPhase[2].order.activeSelf)
        {
            checkPhase[2].order.SetActive(true);
        }

        if(isCheckC)
        {
            checkPhase[2].order.SetActive(false);
        }

        // 回転演出が終わったら次へ
        if (toCheckD)
        {
            playFunc = CheckD;
        }
    }

    private bool isCheckD = false;
    public bool IsCheckD { get { return isCheckD; } set { isCheckD = value; } }
    private bool toCheckE = false;
    public bool ToCheckE { get { return toCheckE; } set { toCheckE = value; } }

    void CheckD()
    {
        if (!checkPhase[3].order.activeSelf)
        {
            checkPhase[3].order.SetActive(true);
        }

        if (isCheckD)
        {
            checkPhase[3].order.SetActive(false);
        }

        // 回転演出が終わったら次へ
        if (toCheckE)
        {
            playFunc = CheckE;
        }
    }

    private bool isCheckTutorialEnd = false;
    public bool IsCheckTutorialEnd { get { return isCheckTutorialEnd; } private  set { isCheckTutorialEnd = value; } }
    void CheckE()
    {
        if (!checkPhase[4].order.activeSelf)
        {
            checkPhase[4].order.SetActive(true);
        }

        if (Input.touchCount >= 1 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            tapCount++;

            for (int i = 0; i < checkPhase[4].objects.Length; i++)
            {
                if (i == tapCount)
                {
                    checkPhase[4].objects[i].SetActive(true);
                }
                else checkPhase[4].objects[i].SetActive(false);
            }

            if (tapCount >= checkPhase[4].objects.Length)
            {
                checkPhase[4].order.SetActive(false);
                tapCount = 0;
                isCheckTutorialEnd = true;

                playFunc = TentativeFunc;
            }
        }
    }

    void SelectA()
    {

    }
}