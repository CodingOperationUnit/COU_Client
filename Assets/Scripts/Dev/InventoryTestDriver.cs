using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryTestDriver : MonoBehaviour
{
    [SerializeField] private long testItemId = 1001;

    [SerializeField] private int testMaterialCount = 2;

    private void Update()
    {
        if (!Debug.isDebugBuild) return;
        if (Keyboard.current == null) return;

        if (Keyboard.current.f1Key.wasPressedThisFrame)
            AddTestMaterials();
    }

    private void AddTestMaterials()
    {
        var inventory = PlayerInventory.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[합성테스트] PlayerInventory가 아직 준비되지 않았습니다.");
            return;
        }

        for (var i = 0; i < testMaterialCount; i++)
            inventory.AddItem(testItemId);

        Debug.Log($"[합성테스트] itemId {testItemId} {testMaterialCount}개 추가 완료. 합성창에서 확인하세요.");
    }

    private void OnGUI()
    {
        if (!Debug.isDebugBuild) return;

        GUI.skin.label.fontSize = 24;
        GUI.Label(new Rect(10, 10, 700, 40),
            $"[F1] 합성 테스트용 장비 추가 (itemId {testItemId} x{testMaterialCount})");
    }
}
