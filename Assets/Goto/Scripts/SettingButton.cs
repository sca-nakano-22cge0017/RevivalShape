using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingButton : MonoBehaviour
{
    [SerializeField]
    GameObject MenuPanel;
    [SerializeField]
    GameObject MainPanel;
    public void MenuOnPush()
    {
        MenuPanel.SetActive(true);
        MainPanel.SetActive(false);
        Time.timeScale = 0;
    }

    public void MenuOffPush()
    {
        MenuPanel.SetActive(false);
        MainPanel.SetActive(true);
        Time.timeScale = 1;
    }
}
