using UnityEngine;

public class HUDTestDriver : MonoBehaviour
{
    private InGameHUD hud;
    private float elapsed;
    private int shownSeconds = -1;
    private int level = 1;
    private int kills;
    private int gold;
    private float exp;

    private void Start()
    {
        hud = UIManager.Instance.Get<InGameHUD>();
        hud.SetLevel(level);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        var seconds = (int)elapsed;
        if (seconds == shownSeconds)
            return;
        shownSeconds = seconds;

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
    }
}
