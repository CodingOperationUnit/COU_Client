using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectWindow : UIView
{
    [SerializeField] private Button[] optionButtons;
    [SerializeField] private TMP_Text[] titleTexts;
    [SerializeField] private TMP_Text[] detailTexts;

    public event Action<int> OnSelected;

    private void Awake()
    {
        for (var i = 0; i < optionButtons.Length; i++)
        {
            var index = i;
            optionButtons[i].onClick.AddListener(() => OnSelected?.Invoke(index));
        }
    }

    public void SetOption(int index, string title, string detail)
    {
        titleTexts[index].text = title;
        detailTexts[index].text = detail;
    }

    public void Show(int count)
    {
        for (var i = 0; i < optionButtons.Length; i++)
            optionButtons[i].gameObject.SetActive(i < count);
        Open();
    }
}
