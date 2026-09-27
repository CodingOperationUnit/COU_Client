using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private float speed = 5f;
    [SerializeField] private InputActionReference inputAction;

    public Vector2 FacingDirection { get; private set; }

    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        FacingDirection = Vector2.right;
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

        if (input.sqrMagnitude > 0.01f)
            FacingDirection = input.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * speed;
    }

    // For Debug (Check Player Direction)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, FacingDirection * 3);
    }
}
