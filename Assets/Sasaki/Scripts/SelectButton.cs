using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
    public static string SelectStage = null;


    //�������{�^���̖��O���擾
    public void ButtonName(string objName)
    {
        //�������\�L�ɂ���
        objName = objName.ToLower();
        SelectStage = objName;
    }
}
