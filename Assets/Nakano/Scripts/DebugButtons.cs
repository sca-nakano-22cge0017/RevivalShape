using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugButtons : MonoBehaviour
{
    public void Reset()
    {
        GameManager.DataReset();
    }

    public void Release()
    {
        GameManager.AllRelease();
    }
}
