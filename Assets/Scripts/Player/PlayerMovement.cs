using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private InputActionReference inputAction;

    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        inputAction.action.Enable();
    }

    private void OnDisable()
    {
        inputAction.action.Disable();
    }

    private void Update()
    {
        input = inputAction.action.ReadValue<Vector2>();


    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }
}
