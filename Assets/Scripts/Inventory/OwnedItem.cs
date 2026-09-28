using System;

public class OwnedItem
{
    public string instanceId;
    public string itemId;
    public bool isEquipped;
    public ItemGrade grade = ItemGrade.Normal;   // 합성/승급으로 올라가는 실제 등급

    public ItemData Data => ItemDatabase.Get(itemId);

    public OwnedItem(string itemId)
    {
        instanceId = Guid.NewGuid().ToString();
        this.itemId = itemId;
    }

    // 팝업에서 이 인덱스의 등급 스킬이 잠겼는지 확인할 때 사용
    public bool IsSkillUnlocked(int skillIndex)
        => Data.GetSkillGrade(skillIndex) <= grade;
}
