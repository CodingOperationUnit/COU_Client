using System;
using TMPro;
using UnityEngine;

public class Popup_Alarm : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tmp_AlarmContext;

    private Action onConfirm;

    public void Show(string message, Action onConfirm = null)
    {
        tmp_AlarmContext.text = message;
        this.onConfirm = onConfirm;
        
        gameObject.SetActive(true);
    }

    public void OnClickConfirm()
    {
        Action callback = onConfirm;

        Close();
        callback?.Invoke();
    }

    public void Close()
    {
        onConfirm = null;
        gameObject.SetActive(false);
    }
}
