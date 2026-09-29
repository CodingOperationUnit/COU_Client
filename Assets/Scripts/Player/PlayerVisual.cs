using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStats))]
public class PlayerVisual : MonoBehaviour
{
    [SerializeField] private Transform visual;
    [SerializeField] private GameObject weapon;

    private PlayerMovement playerMovement;
    private PlayerStats playerStats;

    private bool isFacingRight = true;

    private void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        weapon.SetActive(playerStats.EquippedWeapon != null);
    }

    private void Update()
    {
        float x = playerMovement.FacingDirection.x;

        if (x > 0.01f)
            isFacingRight = true;
        else if (x < -0.01f)
            isFacingRight = false;

        visual.localScale = new Vector3(isFacingRight ? 1f : -1f, 1f, 1f);
    }
}