using System;

public class OwnedItem
{
    public string instanceId;
    public long itemId;
    public bool isEquipped;

    public ItemData Data => ItemDatabase.Get(itemId);
    public ItemGrade Grade => Data.Grade;

    public OwnedItem(long itemId)
    {
        instanceId = Guid.NewGuid().ToString();
        this.itemId = itemId;
    }
}
