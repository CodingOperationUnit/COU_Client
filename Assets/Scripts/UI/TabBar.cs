using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabBar : UIView
{
    [Serializable]
    private class Tab
    {
        public Button button;
        public Image background;
        public LayoutElement layout;
        public RectTransform icon;
        public GameObject label;
        public GameObject badge;
        public TMP_Text badgeText;
        public UIView screen;
    }

    [SerializeField] private Tab[] tabs;
    [SerializeField] private int startIndex = 2;
    [SerializeField] private Color selectedColor;
    [SerializeField] private Color normalColor;
    [SerializeField] private float selectedWidth = 1.9f;
    [SerializeField] private float selectedIconScale = 1.35f;

    private void Awake()
    {
        for (var i = 0; i < tabs.Length; i++)
        {
            var index = i;
            tabs[i].button.onClick.AddListener(() => Select(index));
        }
    }

    private void Start()
        => Select(startIndex);

    public void Select(int index)
    {
        var screen = tabs[index].screen;
        if (screen.Layer != UILayer.HUD)
        {
            screen.Open();
            return;
        }

        for (var i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            var selected = i == index;
            tab.background.color = selected ? selectedColor : normalColor;
            tab.layout.flexibleWidth = selected ? selectedWidth : 1f;
            tab.icon.localScale = Vector3.one * (selected ? selectedIconScale : 1f);
            tab.label.SetActive(selected);

            if (selected)
                tab.screen.Open();
            else
                tab.screen.Close();
        }
    }

    public void SetBadge(int index, string mark)
    {
        tabs[index].badge.SetActive(!string.IsNullOrEmpty(mark));
        tabs[index].badgeText.text = mark;
    }
}
