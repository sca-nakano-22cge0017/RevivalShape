using UnityEngine.SceneManagement;

/// <summary>
/// ���݁E�O�̃V�[������ێ�/�ݒ�
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
