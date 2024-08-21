using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン名保存
/// </summary>
public class SceneController : MonoBehaviour
{
    private void Start()
    {
        SceneName.SetCurrentSceneName();
    }
}

/// <summary>
/// 現在・前のシーン名を保持/設定
/// </summary>
public static class SceneName
{
    private static string currentSceneName;
    private static string lastSceneName;

    public static void SetCurrentSceneName()
    {
        lastSceneName = currentSceneName;
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public static string GetLastSceneName()
    {
        return lastSceneName;
    }

    public static string GetCurrentSceneName()
    {
        return currentSceneName;
    }
}