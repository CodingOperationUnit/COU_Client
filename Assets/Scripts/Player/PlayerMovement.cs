using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerHealth))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private InputActionReference inputAction;
    private PlayerStats playerStats;
    private PlayerHealth playerHealth;

    public Vector2 FacingDirection { get; private set; }

    private Vector2 input;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        FacingDirection = Vector2.right;
        playerStats = GetComponent<PlayerStats>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    private void OnEnable()
    {
        inputAction.action.Enable();
        playerHealth.OnDied += HandleDied;
    }

    private void OnDisable()
    {
        inputAction.action.Disable();
        playerHealth.OnDied -= HandleDied;
    }

    private void Update()
    {
        input = inputAction.action.ReadValue<Vector2>();

        if (input.sqrMagnitude > 0.01f)
            FacingDirection = input.normalized;
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = input * playerStats.Speed;
    }

    private void HandleDied()
    {
        rb.linearVelocity = Vector2.zero;
        enabled = false;
        Debug.Log("이동 중단");
    }

    // For Debug (Check Player Direction)
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, FacingDirection * 3);
    }
}
