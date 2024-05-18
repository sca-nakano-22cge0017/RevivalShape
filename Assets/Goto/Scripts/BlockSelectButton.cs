using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockSelectButton : MonoBehaviour
{
    [SerializeField]
    private GameObject onPanel;
    [SerializeField]
    private GameObject offPanel;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnBlookSelect()
    {
        onPanel.SetActive(false);
        offPanel.SetActive(true);
    }
    public void OffBlookSelect()
    {
        onPanel.SetActive(true);
        offPanel.SetActive(false);
        
    }
}
