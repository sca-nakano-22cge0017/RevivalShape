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

    //�������{�^���̖��O���擾
    public void ButtonName(string objName)
    {
        SelectStage = objName;

        SceneManager.LoadScene("MainScene");
        //Debug�p
        //SceneManager.LoadScene("SampleScene");
    }
}
