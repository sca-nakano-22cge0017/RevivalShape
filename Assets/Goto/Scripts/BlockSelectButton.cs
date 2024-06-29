using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BlockSelectButton : MonoBehaviour
{
    [SerializeField]
    private GameObject onPanel;
    [SerializeField] private GameObject[] offPanels = null;

    // Start is called before the first frame update
    public void StartOpen()
    {
        onPanel.SetActive(true);
        offPanels[0].SetActive(false);
        offPanels[1].SetActive(false);
        offPanels[2].SetActive(false);

    }
    public void PanelClose()
    {
        onPanel.SetActive(false);
        offPanels[1].SetActive(true);
        offPanels[2].SetActive(false);
    }
    public void OnBlookSelect()
    {
        onPanel.SetActive(true);
        offPanels[0].SetActive(false);
        offPanels[1].SetActive(false);
        offPanels[2].SetActive(true);

    }
    public void OffBlookSelect()
    {
        onPanel.SetActive(true);
        offPanels[0].SetActive(true);
        offPanels[2].SetActive(false);

    }
}
