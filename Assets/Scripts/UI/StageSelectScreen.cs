using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StageSelectScreen : UIView, IDragHandler, IEndDragHandler
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private Image illustration;
    [SerializeField] private Image prevIllustration;
    [SerializeField] private Image nextIllustration;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private Button selectButton;
    [SerializeField] private Button backButton;
    [SerializeField] private float swipeThreshold = 100f;

    private StageInfo[] stages;
    private int selected;
    private int index;

    private void Awake()
    {
        selectButton.onClick.AddListener(Select);
        backButton.onClick.AddListener(Close);
    }

    public override void Open()
    {
        base.Open();
        Show(selected);
    }

    public void SetStages(StageInfo[] stages, int selected)
    {
        this.stages = stages;
        this.selected = selected;
    }

    public void OnDrag(PointerEventData eventData)
    {
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var delta = eventData.position.x - eventData.pressPosition.x;
        if (delta <= -swipeThreshold)
            Show(index + 1);
        else if (delta >= swipeThreshold)
            Show(index - 1);
    }

    private void Show(int target)
    {
        index = Mathf.Clamp(target, 0, stages.Length - 1);
        var stage = stages[index];
        titleText.text = stage.name;
        illustration.color = stage.illustrationColor;
        descriptionText.text = stage.description;
        ShowSide(prevIllustration, index - 1);
        ShowSide(nextIllustration, index + 1);
    }

    private void ShowSide(Image side, int sideIndex)
    {
        var exists = sideIndex >= 0 && sideIndex < stages.Length;
        side.gameObject.SetActive(exists);
        if (exists)
            side.color = stages[sideIndex].illustrationColor;
    }

    private void Select()
    {
        selected = index;
        UIManager.Instance.Get<BattleTab>().SetStage(stages[selected]);
        Close();
    }
}
