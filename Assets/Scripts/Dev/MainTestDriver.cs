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
    }

    private void Update()
    {
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            topBar.ShowCoin(coinShown = !coinShown);
    }
}
