using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ResultWindow : MonoBehaviour
{
    [SerializeField] Text[] resultText;
    [SerializeField] Text[] resultText_num;
    [SerializeField] float waitTime = 0.2f;

    private bool dispEnd = false;
    public bool DispEnd { get { return dispEnd; } private set { } }

    private void Awake()
    {
        for (int i = 0; i < resultText.Length; i++)
        {
            resultText[i].enabled = false;
            resultText_num[i].enabled = false;
        }
    }

    void OnEnable()
    {
        dispEnd = false;
        StartCoroutine(Display());
    }

    IEnumerator Display()
    {
        yield return new WaitForSeconds(waitTime);

        for (int i = 0; i < resultText.Length; i++)
        {
            yield return new WaitForSeconds(waitTime);
            resultText[i].enabled = true;
            resultText_num[i].enabled = true;
        }

        dispEnd = true;
        yield break;
    }
}
