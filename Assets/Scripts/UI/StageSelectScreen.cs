using System.Collections.Generic;
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

    private StageData[] stages;
    private Dictionary<int, StageRecordSaveData> records; // PlayerSaveData.stageRecordList를 stageID로 조회하기 쉽게 변환한 것
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

    public void SetStages(StageData[] stages, Dictionary<int, StageRecordSaveData> records, int selected)
    {
        this.stages = stages;
        this.records = records;
        this.selected = selected;
    }

    public void OnDrag(PointerEventData eventData) { }

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
        titleText.text = stage.stageName;
        illustration.color = stage.IllustrationColor;
        descriptionText.text = stage.stageDescription;
        ShowSide(prevIllustration, index - 1);
        ShowSide(nextIllustration, index + 1);
    }

    private void ShowSide(Image side, int sideIndex)
    {
        var exists = sideIndex >= 0 && sideIndex < stages.Length;
        side.gameObject.SetActive(exists);
        if (exists)
            side.color = stages[sideIndex].IllustrationColor;
    }

    private void Select()
    {
        selected = index;
        var stage = stages[selected];
        GameManager.Scene.ChangeScene(GameConstants.SceneNames.BATTLE_SCENE, stage.stageID);
        
        // records.TryGetValue(stage.stageID, out var record);
        // var bestTime = record != null ? Mathf.RoundToInt(record.bestSurvivalSeconds) : 0;
        //
        // UIManager.Instance.Get<BattleTab>().SetStage(stage, bestTime);
        // Close();
    }
}