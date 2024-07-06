using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWindow : MonoBehaviour
{
    [SerializeField] private MissionScore missionScore;
    [SerializeField] private Image[] icons;
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private Sprite[] icons_sp;

    private bool dispEnd = false;
    public bool DispEnd { get { return dispEnd;} private set { } }

    void Awake()
    {
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].enabled = false;
        }
    }

    private void OnEnable()
    {
        dispEnd = false;
        StartCoroutine(Display());
    }

    IEnumerator Display()
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < icons.Length; i++)
        {
            yield return new WaitForSeconds(waitTime);

            icons[i].sprite = missionScore.IsMissionClear[i] ? icons_sp[1] : icons_sp[0];
            icons[i].enabled = true;
        }

        dispEnd = true;
        yield break;
    }
}
