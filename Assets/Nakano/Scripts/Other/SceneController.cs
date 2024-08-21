using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �V�[�����ۑ�
/// </summary>
public class SceneController : MonoBehaviour
{
    private void Start()
    {
        SceneName.SetCurrentSceneName();
    }
}

/// <summary>
/// ���݁E�O�̃V�[������ێ�/�ݒ�
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