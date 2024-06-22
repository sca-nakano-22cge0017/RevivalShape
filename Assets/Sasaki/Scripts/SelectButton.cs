using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SelectButton : MonoBehaviour
{
    public static string SelectStage = null;

    private void Start()
    {

    }

    private void Update()
    {
    }

    //押したボタンの名前を取得
    public void ButtonName(string objName)
    {
        SelectStage = objName;

        SceneManager.LoadScene("MainScene");
        //Debug用
        //SceneManager.LoadScene("SampleScene");
    }
}
