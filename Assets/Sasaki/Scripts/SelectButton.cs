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

    //‰Ÿ‚µ‚½ƒ{ƒ^ƒ“‚Ì–¼‘O‚ðŽæ“¾
    public void ButtonName(string objName)
    {
        SelectStage = objName;

        SceneManager.LoadScene("MainScene");
    }
}
