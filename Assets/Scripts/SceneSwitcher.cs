using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public static void ReturnToMenu()
    {
        if (GameModeManager.Instance)
            Destroy(GameModeManager.Instance.gameObject);

        SceneManager.LoadSceneAsync("Menu");
    }

    public static void LoadScene(string name)
    {
        SceneManager.LoadSceneAsync(name);
    }
}
