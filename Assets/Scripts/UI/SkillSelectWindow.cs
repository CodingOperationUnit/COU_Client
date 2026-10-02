using System;
using UnityEngine;

public class SkillSelectWindow : UIView
{
    [SerializeField] private SkillSelectCard[] cards;
    [SerializeField] private GameObject[] weaponSlotFilled;

    public event Action<int> OnSelected;

    private void Awake()
    {
        for (var i = 0; i < cards.Length; i++)
        {
            var index = i;
            cards[i].OnClicked += () => OnSelected?.Invoke(index);
        }
    }

    public void SetOption(int index, string title, string description, int grade, bool isNew)
        => cards[index].Set(title, description, grade, isNew);

    public void SetWeaponSlots(int filledCount)
    {
        for (var i = 0; i < weaponSlotFilled.Length; i++)
            weaponSlotFilled[i].SetActive(i < filledCount);
    }

    public void Show(int count)
    {
        for (var i = 0; i < cards.Length; i++)
            cards[i].gameObject.SetActive(i < count);
        Open();
    }
}
