using TMPro;
using UnityEngine;

public class LuckTrainRewardCard : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private GameObject[] filledStars;
    [SerializeField] private TMP_Text descriptionText;

    public void Set(string name, int grade, string description)
    {
        nameText.text = name;
        for (var i = 0; i < filledStars.Length; i++)
            filledStars[i].SetActive(i < grade);
        descriptionText.text = description;
    }
}
