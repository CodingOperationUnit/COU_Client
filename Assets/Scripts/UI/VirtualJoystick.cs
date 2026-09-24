using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.OnScreen;

public class VirtualJoystick : OnScreenControl, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [InputControl(layout = "Vector2")]
    [SerializeField] private string controlPath = "<Gamepad>/leftStick";
    [SerializeField] private RectTransform stickBase;
    [SerializeField] private RectTransform knob;
    [SerializeField] private float radius = 165f;

    protected override string controlPathInternal
    {
        get => controlPath;
        set => controlPath = value;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform, eventData.position, eventData.pressEventCamera, out var point);
        stickBase.anchoredPosition = point;
        stickBase.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            stickBase, eventData.position, eventData.pressEventCamera, out var offset);
        offset = Vector2.ClampMagnitude(offset, radius);
        knob.anchoredPosition = offset;
        SendValueToControl(offset / radius);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        knob.anchoredPosition = Vector2.zero;
        stickBase.gameObject.SetActive(false);
        SendValueToControl(Vector2.zero);
    }
}
