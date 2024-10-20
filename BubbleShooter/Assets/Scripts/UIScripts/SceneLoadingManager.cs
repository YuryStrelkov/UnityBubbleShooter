using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Скрипт, который отвечает за загрузку той или иной сцены.
/// Для глобального доступа реализован Singleton
/// </summary>
public class SceneLoadingManager : MonoBehaviour
{
    private static SceneLoadingManager _instance;
    public static SceneLoadingManager Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("SceneLoadingManager");
                _instance = go.AddComponent<SceneLoadingManager>();
            }
            return _instance;
        }
    }
    private void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
    public void ExitGame() => Application.Quit();
    public void LoadMenuScene() => LoadScene("GameMenuScene");
    public void LoadGameScene() => LoadScene("GameScene");
    public void LoadAboutScene() => LoadScene("AboutScene");
}
