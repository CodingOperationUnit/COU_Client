using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] private Image fill;

    public void SetRatio(float ratio)
        => fill.fillAmount = ratio;
}
