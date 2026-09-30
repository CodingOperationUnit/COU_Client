using UnityEngine;
using UnityEngine.UI;

public class GemPurchaseSuccessPopup : UIPopup
{
    [SerializeField] private Button closeButton;
    [SerializeField] private Button dimButton;

    private void Awake()
    {
        closeButton.onClick.AddListener(Close);
        dimButton.onClick.AddListener(Close);
    }

    public void Show()
    {
        Open();
    }
}
