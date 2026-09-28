using UnityEngine;
using UnityEngine.InputSystem;

public class MainTestDriver : MonoBehaviour
{
    private TopBar topBar;
    private bool coinShown;

    private void Start()
    {
        topBar = UIManager.Instance.Get<TopBar>();
        topBar.SetNickname("플레이어 122987861");
        topBar.SetLevel(14);
        topBar.SetExp(0.05f);
        topBar.SetStamina(67, 60);
        topBar.SetCoin(1);
        topBar.SetGem(5750);
        topBar.SetGold(24200);

        var tabBar = UIManager.Instance.Get<TabBar>();
        tabBar.SetBadge(0, "!");
        tabBar.SetBadge(1, "↑");

        var stages = new[]
        {
            new StageInfo { name = "7.더미 스테이지", description = "더미 스테이지 설명입니다.", bestTime = 900, illustrationColor = new Color32(0x7A, 0x86, 0x9A, 0xFF) },
            new StageInfo { name = "8.농경지", description = "한때 사람들이 땀흘려 일하던 농장은 이제 온갖 돌연변이 식물들의 서식지로 변했습니다.", illustrationColor = new Color32(0x6E, 0xA8, 0x3C, 0xFF) },
            new StageInfo { name = "9.목장", description = "이전에는 순했던 이곳의 가축들도 바이러스로 인해 위험한 짐승으로 변했습니다.", illustrationColor = new Color32(0xA8, 0x74, 0x3C, 0xFF) },
            new StageInfo { name = "10.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x8A, 0x5C, 0xB8, 0xFF) }
        };
        UIManager.Instance.Get<StageSelectScreen>().SetStages(stages, 1);
        var battleTab = UIManager.Instance.Get<BattleTab>();
        battleTab.SetStage(stages[1]);
        battleTab.SetStaminaCost(5);
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            topBar.ShowCoin(coinShown = !coinShown);
    }
}
