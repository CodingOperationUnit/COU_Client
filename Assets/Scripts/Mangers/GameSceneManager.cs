using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoSingleton<GameSceneManager>
{
    private int? pendingStageId;
    private long? pendingBattleId;

    public void ChangeScene(string sceneName)
    {
        pendingStageId = null;
        pendingBattleId = null;
        SceneManager.LoadScene(sceneName);
    }

    // 전투 씬 진입: 서버가 발급한 battleId를 결과 요청에 쓰도록 함께 넘긴다
    public void ChangeScene(string sceneName, int stageId, long battleId)
    {
        pendingStageId = stageId;
        pendingBattleId = battleId;
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

    public bool TryConsumePendingBattleId(out long battleId)
    {
        if (pendingBattleId.HasValue)
        {
            battleId = pendingBattleId.Value;
            pendingBattleId = null;
            return true;
        }

        battleId = default;
        return false;
    }
}
