using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EvolutionTab : UIView
{
    [Serializable]
    private class Column
    {
        public EvolutionNodeView nodeTemplate;
        public Image lineTemplate;
        public Color currencyColor;

        [NonSerialized] public EvolutionNodeInfo[] nodes;
        [NonSerialized] public int unlocked;
        [NonSerialized] public int currency;
        [NonSerialized] public List<EvolutionNodeView> views;
        [NonSerialized] public List<Image> lines;
    }

    private const int NodesPerLevel = 3;

    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Button dismissButton;
    [SerializeField] private RectTransform unlockedArea;
    [SerializeField] private EvolutionLevelMark levelMarkTemplate;
    [SerializeField] private Column goldColumn;
    [SerializeField] private Column dnaColumn;
    [SerializeField] private EvolutionTooltip tooltip;
    [SerializeField] private Color unlockedLineColor;
    [SerializeField] private Color lockedLineColor;
    [SerializeField] private float bottomPadding = 330f;
    [SerializeField] private float topPadding = 330f;
    [SerializeField] private float rowSpacing = 300f;

    private int accountLevel;
    private Column selectedColumn;
    private int selectedIndex;

    private void Awake()
    {
        dismissButton.onClick.AddListener(tooltip.Hide);
        tooltip.UnlockButton.onClick.AddListener(Unlock);
    }

    public override void Open()
    {
        base.Open();
        UIManager.Instance.Get<TopBar>().ShowCoin(true);
    }

    public override void Close()
    {
        base.Close();
        tooltip.Hide();
        UIManager.Instance.Get<TopBar>().ShowCoin(false);
    }

    public void Set(EvolutionInfo info)
    {
        accountLevel = info.accountLevel;
        Setup(goldColumn, info.goldNodes, info.unlockedGoldNodes, info.gold, i => i);
        Setup(dnaColumn, info.dnaNodes, info.unlockedDnaNodes, info.dna, i => LevelRow(info.dnaNodes[i].level));

        var maxLevel = info.goldNodes[^1].level;
        for (var level = 1; level <= maxLevel; level++)
        {
            var mark = Instantiate(levelMarkTemplate, levelMarkTemplate.transform.parent);
            mark.gameObject.SetActive(true);
            mark.Set(level, level <= accountLevel);
            Place((RectTransform)mark.transform, RowY(LevelRow(level)));
        }

        var content = scrollRect.content;
        content.sizeDelta = new Vector2(content.sizeDelta.x, RowY(LevelRow(maxLevel)) + topPadding);
        unlockedArea.sizeDelta = new Vector2(unlockedArea.sizeDelta.x, RowY(LevelRow(accountLevel)));

        var viewportHeight = scrollRect.viewport.rect.height;
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01((unlockedArea.sizeDelta.y - viewportHeight / 2) / (content.sizeDelta.y - viewportHeight));
    }

    private void Setup(Column column, EvolutionNodeInfo[] nodes, int unlocked, int currency, Func<int, int> row)
    {
        column.nodes = nodes;
        column.unlocked = unlocked;
        column.currency = currency;
        column.views = new List<EvolutionNodeView>();
        column.lines = new List<Image>();

        for (var i = 0; i < nodes.Length; i++)
        {
            var index = i;
            var view = Instantiate(column.nodeTemplate, column.nodeTemplate.transform.parent);
            view.gameObject.SetActive(true);
            view.Button.onClick.AddListener(() => Select(column, index));
            Place((RectTransform)view.transform, RowY(row(i)));
            column.views.Add(view);

            var line = Instantiate(column.lineTemplate, column.lineTemplate.transform.parent);
            line.gameObject.SetActive(true);
            var from = i == 0 ? 0f : RowY(row(i - 1));
            Place(line.rectTransform, from);
            line.rectTransform.sizeDelta = new Vector2(line.rectTransform.sizeDelta.x, RowY(row(i)) - from);
            column.lines.Add(line);
        }

        Refresh(column);
    }

    private void Refresh(Column column)
    {
        for (var i = 0; i < column.nodes.Length; i++)
        {
            var unlocked = i < column.unlocked;
            column.views[i].Set(column.nodes[i], unlocked);
            column.lines[i].color = unlocked ? unlockedLineColor : lockedLineColor;
        }
    }

    private void Select(Column column, int index)
    {
        selectedColumn = column;
        selectedIndex = index;

        var view = (RectTransform)column.views[index].transform;
        var position = view.anchoredPosition + new Vector2(0f, view.rect.height / 2);
        tooltip.Show(column.nodes[index], position, CanUnlock(column, index), column.currencyColor);
    }

    private bool CanUnlock(Column column, int index)
    {
        var node = column.nodes[index];
        return index == column.unlocked && node.level <= accountLevel && node.cost <= column.currency;
    }

    private void Unlock()
    {
        var column = selectedColumn;
        column.currency -= column.nodes[selectedIndex].cost;
        column.unlocked++;
        Refresh(column);

        var topBar = UIManager.Instance.Get<TopBar>();
        if (column == goldColumn)
            topBar.SetGold(column.currency);
        else
            topBar.SetCoin(column.currency);

        Select(column, selectedIndex);
    }

    private float RowY(int row)
        => bottomPadding + row * rowSpacing;

    private static int LevelRow(int level)
        => level * NodesPerLevel - 1;

    private static void Place(RectTransform target, float y)
        => target.anchoredPosition = new Vector2(target.anchoredPosition.x, y);
}
