using System;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Canvas hudCanvas;
    [SerializeField] private Canvas screenCanvas;
    [SerializeField] private Canvas popupCanvas;
    [SerializeField] private Canvas overlayCanvas;

    private readonly Dictionary<Type, UIView> views = new();
    private readonly List<UIPopup> popups = new();

    private void Awake()
    {
        Instance = this;

        Register(hudCanvas, UILayer.HUD);
        Register(screenCanvas, UILayer.Screen);
        Register(popupCanvas, UILayer.Popup);
        Register(overlayCanvas, UILayer.Overlay);
    }

    private void Register(Canvas canvas, UILayer layer)
    {
        foreach (var view in canvas.GetComponentsInChildren<UIView>(true))
        {
            view.Layer = layer;
            views.Add(view.GetType(), view);
        }
    }

    public T Get<T>() where T : UIView
        => (T)views[typeof(T)];

    public void Open<T>() where T : UIView
        => Get<T>().Open();

    public void Close<T>() where T : UIView
        => Get<T>().Close();

    public void CloseAll()
    {
        foreach (var view in views.Values)
        {
            if (view.Layer != UILayer.HUD && view.IsOpen)
                view.Close();
        }
    }

    public void CloseTopPopup()
    {
        if (popups.Count > 0)
            popups[^1].Close();
    }

    internal void PushPopup(UIPopup popup)
    {
        popups.Remove(popup);
        popups.Add(popup);
    }

    internal void RemovePopup(UIPopup popup)
        => popups.Remove(popup);
}
