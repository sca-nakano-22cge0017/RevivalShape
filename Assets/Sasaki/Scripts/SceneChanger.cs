using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChnger : MonoBehaviour
{
    public void TitleSceneButton()
    {
        SceneManager.LoadScene("SelectScene");
    }

    public void BackTitleButton()
    {
        
        SceneManager.LoadScene("TitleScene");
    }
}
