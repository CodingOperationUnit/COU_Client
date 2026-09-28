using UnityEngine;
using UnityEngine.UI;

public class EvolutionNodeView : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image border;
    [SerializeField] private Image inner;
    [SerializeField] private Image icon;
    [SerializeField] private Color unlockedBorderColor;
    [SerializeField] private Color unlockedInnerColor;
    [SerializeField] private Color lockedBorderColor;
    [SerializeField] private Color lockedInnerColor;
    [SerializeField] private Color lockedIconColor;

    public Button Button => button;

    public void Set(EvolutionNodeInfo node, bool unlocked)
    {
        border.color = unlocked ? unlockedBorderColor : lockedBorderColor;
        inner.color = unlocked ? unlockedInnerColor : lockedInnerColor;
        icon.color = unlocked ? node.iconColor : lockedIconColor;
    }
}
