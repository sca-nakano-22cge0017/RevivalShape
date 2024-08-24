using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// シーン遷移演出再生
/// </summary>
public class SceneLoader : MonoBehaviour
{
    public static void Load(string scene)
    {
        LoadManager loadManager;

        if (FindObjectOfType<LoadManager>() == null)
        {
            // UIが無ければプレハブ生成
            GameObject loadingUIPrefab = Resources.Load<GameObject>("LoadingUI");
            loadManager = Instantiate(loadingUIPrefab).GetComponent<LoadManager>();
        }
        else
        {
            loadManager = FindObjectOfType<LoadManager>();
        }

        // ロード中でないなら
        if (!loadManager.IsLoading)
        {
            loadManager.LoadScene(scene);
        }
    }
}
