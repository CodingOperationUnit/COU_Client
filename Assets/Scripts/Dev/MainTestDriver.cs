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
        topBar.SetGem(PlayerInventory.Instance.Gem);
        topBar.SetGold(PlayerInventory.Instance.Gold);

        PlayerInventory.Instance.OnInventoryChanged += () =>
        {
            topBar.SetGem(PlayerInventory.Instance.Gem);
            topBar.SetGold(PlayerInventory.Instance.Gold);
        };

        var tabBar = UIManager.Instance.Get<TabBar>();
        tabBar.SetBadge(0, "!");
        tabBar.SetBadge(1, "↑");

        var stages = new[]
        {
            new StageInfo { name = "1.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x5A, 0x8C, 0xC8, 0xFF) },
            new StageInfo { name = "2.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0xC8, 0x8C, 0x5A, 0xFF) },
            new StageInfo { name = "3.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x8C, 0xC8, 0x5A, 0xFF) },
            new StageInfo { name = "4.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0xC8, 0x5A, 0x8C, 0xFF) },
            new StageInfo { name = "5.도시 교량", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x3C, 0x46, 0x50, 0xFF) },
            new StageInfo { name = "6.교외 주택지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x3C, 0xA0, 0xD2, 0xFF) },
            new StageInfo { name = "7.더미 스테이지", description = "더미 스테이지 설명입니다.", bestTime = 900, illustrationColor = new Color32(0x7A, 0x86, 0x9A, 0xFF) },
            new StageInfo { name = "8.농경지", description = "한때 사람들이 땀흘려 일하던 농장은 이제 온갖 돌연변이 식물들의 서식지로 변했습니다.", illustrationColor = new Color32(0x6E, 0xA8, 0x3C, 0xFF) },
            new StageInfo { name = "9.목장", description = "이전에는 순했던 이곳의 가축들도 바이러스로 인해 위험한 짐승으로 변했습니다.", illustrationColor = new Color32(0xA8, 0x74, 0x3C, 0xFF) },
            new StageInfo { name = "10.더미 스테이지", description = "더미 스테이지 설명입니다.", illustrationColor = new Color32(0x8A, 0x5C, 0xB8, 0xFF) }
        };
        UIManager.Instance.Get<StageSelectScreen>().SetStages(stages, 7);
        var battleTab = UIManager.Instance.Get<BattleTab>();
        battleTab.SetStage(stages[7]);
        battleTab.SetStaminaCost(5);

        var conditions = new[]
        {
            new ChallengeCondition { name = "IQ 2배", description = "레벨업에 필요한 경험치 50% 감소" },
            new ChallengeCondition { name = "쌍둥이", description = "매번 2마리 보스 출현" }
        };
        var rewards = new[]
        {
            new ChallengeReward { grade = Grade.Elite, iconColor = new Color32(0xFF, 0xC6, 0x1A, 0xFF), amount = 2 },
            new ChallengeReward { techPart = true },
            new ChallengeReward { grade = Grade.Epic, iconColor = new Color32(0xFF, 0x8C, 0x1A, 0xFF), amount = 1 },
            new ChallengeReward { grade = Grade.Elite, iconColor = new Color32(0x3C, 0xC8, 0x3C, 0xFF), amount = 200 }
        };
        var chapters = new ChallengeChapterInfo[8];
        for (var i = 0; i < chapters.Length; i++)
        {
            var challenge = new ChallengeInfo { conditions = conditions, rewards = rewards, rewarded = i < 5 };
            chapters[i] = new ChallengeChapterInfo { number = i + 1, stage = stages[i], challenges = new[] { challenge, challenge, challenge } };
        }
        UIManager.Instance.Get<ChallengeTab>().SetChapters(chapters);

        var goldNodeTypes = new[]
        {
            new EvolutionNodeInfo { name = "공격력", value = "ATK +20", description = "더미 설명입니다.", iconColor = new Color32(0xE8, 0x28, 0x4F, 0xFF) },
            new EvolutionNodeInfo { name = "체력", value = "HP +60", description = "체력이 세면 파워도 셉니다.", iconColor = new Color32(0xFF, 0xC6, 0x1A, 0xFF) },
            new EvolutionNodeInfo { name = "방어력", value = "DEF +10", description = "더미 설명입니다.", iconColor = new Color32(0x0A, 0x90, 0xFF, 0xFF) },
            new EvolutionNodeInfo { name = "회복", value = "HP 회복 +5", description = "더미 설명입니다.", iconColor = new Color32(0xC8, 0x64, 0x3C, 0xFF) }
        };
        var goldNodes = new EvolutionNodeInfo[40 * 3];
        for (var i = 0; i < goldNodes.Length; i++)
        {
            goldNodes[i] = goldNodeTypes[i % goldNodeTypes.Length];
            goldNodes[i].level = i / 3 + 1;
            goldNodes[i].cost = goldNodes[i].level * 100;
        }
        var dnaNodeTypes = new[]
        {
            new EvolutionNodeInfo { name = "무기 숙련", value = "ATK +5%", description = "더미 설명입니다.", iconColor = new Color32(0xA0, 0xA7, 0xB4, 0xFF), cost = 1 },
            new EvolutionNodeInfo { name = "지혜", value = "EXP +10%", description = "더미 설명입니다.", iconColor = new Color32(0xFF, 0xC6, 0x1A, 0xFF), cost = 1 }
        };
        var dnaLevels = new[] { 3, 5, 7, 9, 11, 13, 15, 20, 25, 30, 35, 40 };
        var dnaNodes = new EvolutionNodeInfo[dnaLevels.Length];
        for (var i = 0; i < dnaNodes.Length; i++)
        {
            dnaNodes[i] = dnaNodeTypes[i % dnaNodeTypes.Length];
            dnaNodes[i].level = dnaLevels[i];
        }
        UIManager.Instance.Get<EvolutionTab>().Set(new EvolutionInfo
        {
            accountLevel = 14,
            gold = 24200,
            dna = 1,
            goldNodes = goldNodes,
            dnaNodes = dnaNodes,
            unlockedGoldNodes = 40,
            unlockedDnaNodes = 5
        });
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            topBar.ShowCoin(coinShown = !coinShown);
    }
}
