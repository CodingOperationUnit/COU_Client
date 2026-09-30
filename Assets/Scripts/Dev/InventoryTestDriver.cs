using UnityEngine;

public class InventoryTestDriver : MonoBehaviour
{
    private void Start()
    {
        if (PlayerInventory.Instance.Items.Count > 0) return;
    }
}
