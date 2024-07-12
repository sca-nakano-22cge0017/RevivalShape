using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveObject : MonoBehaviour
{

    // Start is called before the first frame update

    private void Awake()
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("SaveSetingMenu");

            if (objects.Length> 1)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
