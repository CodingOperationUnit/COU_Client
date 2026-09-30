using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoSingleton<GameSceneManager>
{
    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
