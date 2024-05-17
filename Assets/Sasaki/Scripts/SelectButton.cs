using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectButton : MonoBehaviour
{
    public static string SelectStage = null;


    //押したボタンの名前を取得
    public void ButtonName(string objName)
    {
        //小文字表記にする
        objName = objName.ToLower();
        SelectStage = objName;
    }
}
