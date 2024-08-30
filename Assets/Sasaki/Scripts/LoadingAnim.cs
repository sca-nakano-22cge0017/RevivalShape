using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingAnim : MonoBehaviour
{
    [SerializeField] private Animator[] animator;
    [SerializeField] private float times = 0.3f;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(AnimMove());
    }

    private IEnumerator AnimMove()
    {
        for (int i = 0; i < animator.Length; i++)
        {
            animator[i].SetBool("LoadBool", true);
            yield return new WaitForSeconds(times);
        }
    }
}
