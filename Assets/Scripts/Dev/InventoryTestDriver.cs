using UnityEngine;

public class InventoryTestDriver : MonoBehaviour
{
    private void Start()
    {
        PlayerInventory.Instance.AddItem(2001); 
        PlayerInventory.Instance.AddItem(6001);
        PlayerInventory.Instance.AddItem(2001); // °¡Á× º§Æ®
        PlayerInventory.Instance.AddItem(6001); // ³°Àº °Ë
        PlayerInventory.Instance.AddItem(6002); // ¿¬¹ß ±ÇÃÑ
        PlayerInventory.Instance.AddItem(6003); // ±×¸²ÀÚ Äí³ªÀÌ
    }
}
