using UnityEngine;
using UnityEngine.UI;

public class SelectPhaseButton : MonoBehaviour
{
    int inputNum = 0;

    /// <summary>
    /// �e�{�^���ɓ��͂��ꂽ��
    /// </summary>
    public int InputNum
    {
        get { return inputNum; }
        set { inputNum = value; }
    }

    Text thisText;

    const int input_max = 10; // ���͂ł���ő�l

    private void Start()
    {
        // �{�^���̎q�I�u�W�F�N�g��Text���擾
        thisText = transform.GetChild(0).gameObject.GetComponent<Text>();
    }

    /// <summary>
    /// �{�^������������J�E���g�𑝂₷
    /// </summary>
    public void CountUp()
    {
        inputNum++;

        if(inputNum > input_max) inputNum = 0;

        thisText.text = inputNum.ToString(); // �\���ύX
    }
}
