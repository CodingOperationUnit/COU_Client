using TMPro;
using UnityEngine;

public class AlarmView : UIView
{
    [SerializeField] private TMP_Text messageText;

    public void Show(string message)
    {
        messageText.text = message;
        Open();
    }
}
