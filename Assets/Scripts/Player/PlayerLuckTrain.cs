using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkillController))]
public class PlayerLuckTrain : MonoBehaviour
{
    private const int SlotCount = 16;

    [SerializeField][Min(0)] private int goldMin = 100;   // 임시 값
    [SerializeField][Min(0)] private int goldMax = 300;   // 임시 값

    private SkillController skillController;

    private LuckTrainWindow window;
    private SkillSelectWindow selectWindow;
    private BattleResultWindow resultWindow;
    private bool windowsResolved;

    private int pending;
    private bool isShowing;
    private int rewardGold;

    private readonly int[] slotSkillIds = new int[SlotCount];
    private readonly List<int> selectedSkillIds = new();

    public int Pending => pending;
    public bool IsShowing => isShowing;

    private void Awake()
    {
        skillController = GetComponent<SkillController>();
    }

    private void OnDestroy()
    {
        if (window != null)
            window.OnFinished -= HandleFinished;
    }

    // 행운상자 획득
    public void Add()
    {
        pending++;
        Debug.Log("[PlayerLuckTrain] 행운상자 획득, 대기: " + pending);
    }

    private void Update()
    {
        if (isShowing)
        {
            if (IsBattleEnded())
                Abort();
            return;
        }

        if (pending == 0) return;

        if (!ResolveWindows())
        {
            Debug.LogWarning("[PlayerLuckTrain] 행운열차 창을 찾을 수 없어 대기분을 버립니다. (전투 씬이 아닌 경우 정상)");
            pending = 0;
            return;
        }

        if (IsBattleEnded())
        {
            pending = 0;
            return;
        }

        // 레벨업 선택 창이 우선 (WP03)
        if (selectWindow != null && selectWindow.IsOpen) return;

        ShowNext();
    }

    private void ShowNext()
    {
        pending--;

        var candidates = new List<SkillBase>();
        foreach (var skill in skillController.ActiveSkills)
        {
            if (skill.Level < SkillBase.MaxLevel)
                candidates.Add(skill);
        }

        if (candidates.Count == 0)
        {
            Debug.Log("[PlayerLuckTrain] 등급을 올릴 스킬이 없어 행운열차를 건너뜁니다.");
            return;
        }

        // 중복 허용
        for (int i = 0; i < SlotCount; i++)
        {
            int skillId = candidates[Random.Range(0, candidates.Count)].SkillId;
            slotSkillIds[i] = skillId;
            window.SetSlot(i, SkillDataBase.Get(skillId).Name);
        }

        int[] selected = { Random.Range(0, SlotCount) };

        selectedSkillIds.Clear();
        var previewLevels = new Dictionary<int, int>();

        for (int i = 0; i < selected.Length; i++)
        {
            int skillId = slotSkillIds[selected[i]];
            selectedSkillIds.Add(skillId);

            if (!previewLevels.TryGetValue(skillId, out int level))
                level = FindLevel(skillId);

            level++;
            previewLevels[skillId] = level;

            var data = SkillDataBase.Get(skillId);
            window.SetReward(i, data.Name, level, "Lv." + level);
        }

        rewardGold = Random.Range(goldMin, goldMax + 1) * selected.Length;

        BattleManager.Instance?.RequestPause(this);
        window.Show(selected, selected.Length, rewardGold);
        isShowing = true;
    }

    private void HandleFinished()
    {
        foreach (int skillId in selectedSkillIds)
        {
            bool result = skillController.LevelUpSkill(skillId);
            Debug.Log("[PlayerLuckTrain] 스킬 " + skillId + " 등급 상승: " + (result ? "성공" : "실패"));
        }

        // TODO: 배틀 매니저에 전투 골드 추가 함수가 생기면 교체
        Debug.Log("[PlayerLuckTrain] 행운열차 골드 " + rewardGold + " 전달 예정 (배틀 매니저 함수 대기)");

        BattleManager.Instance?.ReleasePause(this);
        isShowing = false;
    }

    private void Abort()
    {
        if (window != null && window.IsOpen)
            window.Close();

        BattleManager.Instance?.ReleasePause(this);
        isShowing = false;
        pending = 0;
        Debug.Log("[PlayerLuckTrain] 전투 종료로 행운열차 정리");
    }

    private bool IsBattleEnded()
        => resultWindow != null && resultWindow.IsOpen;

    private int FindLevel(int skillId)
    {
        foreach (var skill in skillController.ActiveSkills)
        {
            if (skill.SkillId == skillId) return skill.Level;
        }
        return 0;
    }

    private bool ResolveWindows()
    {
        if (windowsResolved) return window != null;
        windowsResolved = true;

        window = TryGetView<LuckTrainWindow>();
        selectWindow = TryGetView<SkillSelectWindow>();
        resultWindow = TryGetView<BattleResultWindow>();

        if (window != null)
            window.OnFinished += HandleFinished;

        return window != null;
    }

    private static T TryGetView<T>() where T : UIView
    {
        try
        {
            return UIManager.Instance.Get<T>();
        }
        catch (KeyNotFoundException)
        {
            return null;
        }
    }

    [ContextMenu("TestLuckyBox")]
    private void TestLuckyBox() => Add();
}