using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillSelectCard : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private GameObject newTag;
    [SerializeField] private TMP_Text[] stars;
    [SerializeField] private Color filledColor = new(1f, 0.776f, 0.102f);
    [SerializeField] private Color emptyColor = new(0.18f, 0.188f, 0.251f);
    [SerializeField] private float blinkSpeed = 2f;

    private int blinkIndex = -1;

    public event Action OnClicked;

    private void Awake()
    {
        button.onClick.AddListener(() => OnClicked?.Invoke());
    }

    private void Update()
    {
        if (blinkIndex < 0) return;

        var color = filledColor;
        color.a = Mathf.Lerp(0.25f, 1f, Mathf.PingPong(Time.unscaledTime * blinkSpeed, 1f));
        stars[blinkIndex].color = color;
    }

    // grade: 선택하면 도달하는 등급(채울 별 개수). 마지막으로 채워지는 별이 깜빡인다
    public void Set(string title, string description, int grade, bool isNew)
    {
        nameText.text = title;
        descriptionText.text = description;
        newTag.SetActive(isNew);

        for (var i = 0; i < stars.Length; i++)
            stars[i].color = i < grade ? filledColor : emptyColor;
        blinkIndex = grade > 0 ? grade - 1 : -1;
    }
}
