using UnityEngine;

public abstract class UIView : MonoBehaviour
{
    public UILayer Layer { get; internal set; }

    public bool IsOpen => gameObject.activeSelf;

    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    public virtual void Close()
    {
        gameObject.SetActive(false);
    }
}
