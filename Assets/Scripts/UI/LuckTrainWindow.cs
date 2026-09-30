using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckTrainWindow : UIView
{
    private enum State
    {
        Idle,
        Spin,
        Reveal,
        Result
    }

    [SerializeField] private TMP_Text[] slotNames;
    [SerializeField] private GameObject[] slotHighlights;
    [SerializeField] private TMP_Text goldText;
    [SerializeField] private Button startButton;
    [SerializeField] private GameObject skipHint;
    [SerializeField] private Button skipArea;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private LuckTrainRewardCard[] rewardCards;
    [SerializeField] private Button continueButton;
    [SerializeField] private float spinDuration = 2.5f;
    [SerializeField] private int spinLaps = 2;
    [SerializeField] private float revealInterval = 0.3f;

    public event Action OnFinished;

    private State state;
    private int[] selected;
    private int count;
    private int gold;
    private int totalSteps;
    private int highlighted;
    private int revealed;
    private float timer;

    private void Awake()
    {
        startButton.onClick.AddListener(StartSpin);
        skipArea.onClick.AddListener(ShowResult);
        continueButton.onClick.AddListener(Finish);
    }

    private void Update()
    {
        if (state == State.Spin)
            UpdateSpin();
        else if (state == State.Reveal)
            UpdateReveal();
    }

    public void SetSlot(int index, string name)
        => slotNames[index].text = name;

    public void SetReward(int index, string name, int grade, string description)
        => rewardCards[index].Set(name, grade, description);

    public void Show(int[] selected, int count, int gold)
    {
        this.selected = selected;
        this.count = count;
        this.gold = gold;
        state = State.Idle;

        foreach (var highlight in slotHighlights)
            highlight.SetActive(false);
        goldText.text = "0";
        startButton.gameObject.SetActive(true);
        skipHint.SetActive(false);
        skipArea.gameObject.SetActive(false);
        resultPanel.SetActive(false);
        for (var i = 0; i < rewardCards.Length; i++)
            rewardCards[i].gameObject.SetActive(i < count);
        Open();
    }

    private void StartSpin()
    {
        startButton.gameObject.SetActive(false);
        skipHint.SetActive(true);
        skipArea.gameObject.SetActive(true);

        totalSteps = spinLaps * slotHighlights.Length + selected[0];
        highlighted = 0;
        slotHighlights[highlighted].SetActive(true);
        timer = 0f;
        state = State.Spin;
    }

    private void UpdateSpin()
    {
        timer += Time.unscaledDeltaTime;
        var t = Mathf.Min(timer / spinDuration, 1f);
        var progress = 1f - (1f - t) * (1f - t);

        var index = Mathf.FloorToInt(totalSteps * progress) % slotHighlights.Length;
        if (index != highlighted)
        {
            slotHighlights[highlighted].SetActive(false);
            slotHighlights[index].SetActive(true);
            highlighted = index;
        }
        goldText.text = Mathf.FloorToInt(gold * progress).ToString();

        if (t < 1f) return;
        revealed = 1;
        timer = 0f;
        state = State.Reveal;
    }

    private void UpdateReveal()
    {
        timer += Time.unscaledDeltaTime;
        if (timer < revealInterval) return;
        timer = 0f;

        if (revealed < count)
            slotHighlights[selected[revealed++]].SetActive(true);
        else
            ShowResult();
    }

    private void ShowResult()
    {
        foreach (var highlight in slotHighlights)
            highlight.SetActive(false);
        for (var i = 0; i < count; i++)
            slotHighlights[selected[i]].SetActive(true);
        goldText.text = gold.ToString();

        skipArea.gameObject.SetActive(false);
        resultPanel.SetActive(true);
        state = State.Result;
    }

    private void Finish()
    {
        Close();
        OnFinished?.Invoke();
    }
}
