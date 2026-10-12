using System.Globalization;
using TMPro;
using UnityEngine;

// 전투력 표시: 텍스트에 붙이면 장비·진화 변경 시 스스로 갱신
[RequireComponent(typeof(TMP_Text))]
public class CombatPowerView : MonoBehaviour
{
    [SerializeField] private string format = "전투력 {0}";

    private TMP_Text label;
    private bool isInventorySubscribed;
    private bool isPlayerDataSubscribed;

    private void Awake()
    {
        label = GetComponent<TMP_Text>();
    }

    private void Start()
    {
        if (PlayerInventory.Instance != null)
        {
            PlayerInventory.Instance.OnInventoryChanged += Refresh;
            isInventorySubscribed = true;
        }

        if (GameManager.PlayerData != null)
        {
            GameManager.PlayerData.OnPlayerDataChanged += OnPlayerDataChanged;
            isPlayerDataSubscribed = true;
        }

        Refresh();
    }

    private void OnEnable()
    {
        if (label != null) Refresh();   // 닫혀 있는 동안 바뀐 값 반영
    }

    private void OnDestroy()
    {
        if (isInventorySubscribed && PlayerInventory.Instance != null)
            PlayerInventory.Instance.OnInventoryChanged -= Refresh;

        if (isPlayerDataSubscribed && GameManager.PlayerData != null)
            GameManager.PlayerData.OnPlayerDataChanged -= OnPlayerDataChanged;
    }

    private void OnPlayerDataChanged(PlayerSaveData _) => Refresh();

    private void Refresh()
    {
        if (GameManager.PlayerData == null || !GameManager.PlayerData.isPlayerDataLoaded)
        {
            label.text = string.Format(format, "-");
            return;
        }

        var stat = PlayerStatCalculator.CalculateCurrent();

        if (!stat.HasCombatPowerData)
        {
            label.text = string.Format(format, "-");
            Debug.LogWarning("[CombatPowerView] 전투력 데이터가 없습니다.", this);
            return;
        }

        // 천 단위 쉼표 (2,325)
        label.text = string.Format(format, stat.CombatPower.ToString("N0", CultureInfo.InvariantCulture));
    }
}