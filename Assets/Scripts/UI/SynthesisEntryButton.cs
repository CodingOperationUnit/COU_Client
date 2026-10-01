using System.Linq;
using UnityEngine;
using UnityEngine.UI;


public class SynthesisEntryButton : MonoBehaviour
{
    private void Awake()
        => GetComponent<Button>().onClick.AddListener(OnClicked);

    private void OnClicked()
    {
        var inventory = PlayerInventory.Instance;
        if (inventory == null)
        {
            Debug.LogWarning("[합성] PlayerInventory가 아직 준비되지 않았습니다.");
            return;
        }

        var target = inventory.Items.FirstOrDefault(i => inventory.CanSynthesize(i))
                   ?? inventory.Items.FirstOrDefault();

        UIManager.Instance.Get<SynthesisWindow>().Show(target);
    }
}
