using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoSingleton<UIManager>
{
    [SerializeField] private Canvas overlayCanvas;

    private readonly Dictionary<Type, UIView> views = new();
    private readonly List<UIPopup> popups = new();

    private InputAction cancelAction;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
        => Instantiate(Resources.Load<UIManager>("UI/UIManager"));

    protected override void Awake()
    {
        base.Awake();

        Register(overlayCanvas, UILayer.Overlay);

        cancelAction = InputSystem.actions.FindAction("UI/Cancel");
    }

    private void Update()
    {
        if (cancelAction.WasPressedThisFrame())
            CloseTopPopup();
    }

    internal void Register(Canvas canvas, UILayer layer)
    {
        foreach (var view in canvas.GetComponentsInChildren<UIView>(true))
        {
            view.Layer = layer;
            views[view.GetType()] = view;
        }
    }

    internal void Unregister(IEnumerable<UIView> targets)
    {
        foreach (var view in targets)
        {
            if (views.TryGetValue(view.GetType(), out var registered) && registered == view)
                views.Remove(view.GetType());

            if (view is UIPopup popup)
                popups.Remove(popup);
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
