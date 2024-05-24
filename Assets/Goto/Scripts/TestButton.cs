using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestButton : MonoBehaviour
{
    public StageController stageController;
    [SerializeField] private Toggle[] toggles = null;
    // Start is called before the first frame update
    public void OnNext()
    {
        toggles[1].isOn=true;
    }
    public void OnPlayNext()
    {
        toggles[2].isOn = true;
    }
    public void BackToggle()
    {
        toggles[0].isOn = true;
    }
}
