using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDebug : MonoBehaviour
{
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private int testDamage = 50;
    [SerializeField] private int testHeal = 20;

    private PlayerVisual playerVisual;

    private void Awake()
    {
        if (playerHealth == null)
        {
            Debug.LogError("[PlayerDebug] Player Health가 연결되지 않았습니다. Inspector에서 Player를 연결하세요.", this);
            enabled = false;
            return;
        }

        playerVisual = playerHealth.GetComponent<PlayerVisual>();
    }

    private void Update()
    {
        if (!Debug.isDebugBuild) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
            playerHealth.GetDamage(testDamage);

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
            playerHealth.Heal(testHeal);

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
            playerHealth.IncreaseLife();

        if (Keyboard.current.digit4Key.wasPressedThisFrame && playerVisual != null)
            playerVisual.PlayAttackMotion();
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(10, 10, 500, 300),
            "[1] Damage\n[2] Heal\n[3] Restore Life\n[4] Attack Motion\n\n" +
            "HP : " + playerHealth.CurrentHealth + " / " + playerHealth.MaxHealth + "\n\n" +
            "Life : " + playerHealth.CurrentLives + " / " + playerHealth.MaxLives);
    }
}