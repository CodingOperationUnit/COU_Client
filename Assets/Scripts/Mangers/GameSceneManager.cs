using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoSingleton<GameSceneManager>
{
    private int? pendingStageID;
    
    public void ChangeScene(string sceneName)
    {
        pendingStageID = null;
        SceneManager.LoadScene(sceneName);
    }

    public void ChangeScene(string sceneName, int stageID)
    {
        pendingStageID = stageID;
        SceneManager.LoadScene(sceneName);
    }
    
    public bool TryConsumePendingStageId(out int stageID)
    {
        if (pendingStageID.HasValue)
        {
            stageID = pendingStageID.Value;
            pendingStageID = null;
            return true;
        }

        stageID = default;
        return false;
    }
}
