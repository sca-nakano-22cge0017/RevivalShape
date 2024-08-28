using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChnger : MonoBehaviour
{
    public void TitleSceneButton()
    {
        
        SceneLoader.Load("SelectScene");
    }

    public void BackTitleButton()
    {
        
        SceneLoader.Load("TitleScene");
    }
}
