using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MissionWindow : MonoBehaviour
{
    [SerializeField] private MissionScore missionScore;
    [SerializeField] private Image[] icons;
    [SerializeField] private float waitTime = 0.2f;
    [SerializeField] private Animator[] rotateAnim;

    private bool dispEnd = false;
    public bool DispEnd { get { return dispEnd;} private set { } }

    private IEnumerator disp;

    // SE
    private SoundManager sm;

    void Awake()
    {
        disp = Display();

        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].enabled = false;
        }

        sm = FindObjectOfType<SoundManager>();
    }

    private void OnEnable()
    {
        dispEnd = false;

        StartCoroutine(disp);
    }

    IEnumerator Display()
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < icons.Length; i++)
        {
            yield return new WaitForSeconds(waitTime);

            if(missionScore.IsMissionClear[i])
            {
                icons[i].enabled = true;
                rotateAnim[i].SetTrigger("Start");
                SEPlay();
            }
        }

        dispEnd = true;
        yield break;
    }

    void SEPlay()
    {
        if( sm != null)
        {
            sm.SEPlay5();
        }
    }
}
