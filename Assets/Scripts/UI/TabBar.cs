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
    [SerializeField] private UIColor selectedColor = UIColor.Surface;
    [SerializeField] private UIColor normalColor = UIColor.Base;
    [SerializeField] private float selectedWidth = 1.9f;
    [SerializeField] private float selectedIconScale = 1.35f;

    private UIManager manager;
    private int selectedIndex;

    private void Awake()
    {
        for (var i = 0; i < tabs.Length; i++)
        {
            var index = i;
            tabs[i].button.onClick.AddListener(() => Select(index));
        }

        manager = UIManager.Instance;
        manager.ThemeChanged += ApplyColors;
    }

    private void Start()
        => Select(startIndex);

    private void OnDestroy()
    {
        if (manager != null)
            manager.ThemeChanged -= ApplyColors;
    }

    public void Select(int index)
    {
        var screen = tabs[index].screen;
        if (screen.Layer != UILayer.HUD)
        {
            screen.Open();
            return;
        }

        selectedIndex = index;
        ApplyColors();

        for (var i = 0; i < tabs.Length; i++)
        {
            var tab = tabs[i];
            var selected = i == index;
            tab.layout.flexibleWidth = selected ? selectedWidth : 1f;
            tab.icon.localScale = Vector3.one * (selected ? selectedIconScale : 1f);
            tab.label.SetActive(selected);

            if (selected)
                tab.screen.Open();
            else
                tab.screen.Close();
        }
    }

    private void ApplyColors()
    {
        for (var i = 0; i < tabs.Length; i++)
            tabs[i].background.color = UIPalette.Get(i == selectedIndex ? selectedColor : normalColor);
    }

    public void SetBadge(int index, string mark)
    {
        tabs[index].badge.SetActive(!string.IsNullOrEmpty(mark));
        tabs[index].badgeText.text = mark;
    }
}
