using UnityEngine;
using UnityEngine.InputSystem;

public class HUDTestDriver : MonoBehaviour
{
    [SerializeField] private GameObject dummyBoss;
    [SerializeField] private int bossSeconds = 10;
    [SerializeField] private int alarmLeadSeconds = 5;

    private InGameHUD hud;
    private AlarmView alarm;
    private PauseWindow pause;
    private BattleResultWindow result;
    private float elapsed;
    private int shownSeconds = -1;
    private int level = 1;
    private int kills;
    private int gold;
    private float exp;
    private int bossHp;

    private void Start()
    {
        hud = UIManager.Instance.Get<InGameHUD>();
        alarm = UIManager.Instance.Get<AlarmView>();
        pause = UIManager.Instance.Get<PauseWindow>();
        result = UIManager.Instance.Get<BattleResultWindow>();
        hud.SetLevel(level);
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            ShowResult(true);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            ShowResult(false);

        elapsed += Time.deltaTime;
        var seconds = (int)elapsed;
        if (seconds == shownSeconds)
            return;
        shownSeconds = seconds;

        if (seconds == bossSeconds - alarmLeadSeconds)
            alarm.Show("보스 습격");
        else if (seconds == bossSeconds)
        {
            alarm.Close();
            dummyBoss.SetActive(true);
            bossHp = 10;
            hud.ShowBoss("좀비 대장");
            hud.SetBossHp(1f);
        }
        else if (dummyBoss.activeSelf)
        {
            if (--bossHp == 0)
            {
                dummyBoss.SetActive(false);
                hud.HideBoss();
            }
            else
                hud.SetBossHp(bossHp / 10f);
        }

        kills += Random.Range(0, 30);
        gold += Random.Range(0, 20);
        exp += 0.2f;
        if (exp >= 1f)
        {
            exp = 0f;
            hud.SetLevel(++level);
        }

        hud.SetTime(seconds);
        hud.SetKillCount(kills);
        hud.SetGold(gold);
        hud.SetExp(exp);
        pause.SetKillCount(kills);
        pause.SetGold(gold);
    }

    private void ShowResult(bool victory)
    {
        result.SetTime(shownSeconds);
        result.SetChapter(victory ? 5 : 6);
        result.SetBestTime(victory ? 900 : 520);
        result.SetKillCount(kills);
        result.SetBoxCount(victory ? 16 : 0);
        result.SetGold(victory ? 83700 : 10000);
        result.SetExp(victory ? 5600 : 1500);
        result.Show(victory);
    }
}
