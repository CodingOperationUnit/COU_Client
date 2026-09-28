using UnityEngine;

public class InventoryTestDriver : MonoBehaviour
{
    private void Start()
    {
        PlayerInventory.Instance.AddItem(2001); 
        PlayerInventory.Instance.AddItem(6001);
    }
}
