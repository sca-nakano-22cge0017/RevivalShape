using UnityEngine.SceneManagement;

/// <summary>
/// 現在・前のシーン名を保持/設定
/// </summary>
public static class SceneController
{
    private static string currentSceneName = "Title";
    private static string lastSceneName = "Title";

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
