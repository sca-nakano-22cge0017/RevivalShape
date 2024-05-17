using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChnger : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) 
            && SceneManager.GetActiveScene().name == "TitleScene")
        {
            SceneManager.LoadScene("SelectScene");
        }
    }

    public void BackTitleButton()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
