using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoSingleton<GameSceneManager>
{
    private int? pendingStageId;
    
    public void ChangeScene(string sceneName)
    {
        pendingStageId = null;
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(string sceneName, int stageId)
    {
        pendingStageId = stageId;
        SceneManager.LoadScene(sceneName);
    }
    
    public bool TryConsumePendingStageId(out int stageId)
    {
        if (pendingStageId.HasValue)
        {
            stageId = pendingStageId.Value;
            pendingStageId = null;
            return true;
        }

        stageId = default;
        return false;
    }
}
