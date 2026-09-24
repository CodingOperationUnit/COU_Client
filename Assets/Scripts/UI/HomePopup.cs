using UnityEngine;
using UnityEngine.UI;

public class HomePopup : UIPopup
{
    [SerializeField] private Button continueButton;

    private void Awake()
    {
        continueButton.onClick.AddListener(Close);
    }
}
