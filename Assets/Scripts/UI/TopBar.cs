using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TopBar : UIView
{
    [SerializeField] private Button profileButton;
    [SerializeField] private TMP_Text nicknameText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private Image expFill;
    [SerializeField] private GameObject staminaGroup;
    [SerializeField] private TMP_Text staminaText;
    [SerializeField] private GameObject coinGroup;
    [SerializeField] private TMP_Text coinText;
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text goldText;

    private void Awake()
    {
        profileButton.onClick.AddListener(() => UIManager.Instance.Open<ProfilePopup>());
    }

    public void SetNickname(string nickname)
        => nicknameText.text = nickname;

    public void SetLevel(int level)
        => levelText.text = level.ToString();

    public void SetExp(float ratio)
        => expFill.fillAmount = ratio;

    public void SetStamina(int current, int max)
        => staminaText.text = $"{current}/{max}";

    public void SetCoin(int coin)
        => coinText.text = coin.ToString();

    public void SetGem(int gem)
        => gemText.text = gem.ToString();

    public void SetGold(int gold)
        => goldText.text = FormatAmount(gold);

    public void ShowCoin(bool show)
    {
        staminaGroup.SetActive(!show);
        coinGroup.SetActive(show);
    }

    private static string FormatAmount(int amount)
        => amount >= 10000 ? $"{amount / 1000f:0.#}K" : amount.ToString();
}
