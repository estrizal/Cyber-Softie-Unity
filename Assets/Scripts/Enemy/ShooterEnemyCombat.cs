using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class ShooterEnemyCombat : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject player;
    private Gun gun;
    
    [Header("Combat Settings")]
    public float attackRange = 15f;
    public float attackCooldown = 1f;
    
    [Header("References")]
    public Transform weaponHolder;
    
    private bool isShooting;
    private const float heightOffset = 0.5f;

    [Header("Possession Settings")]
    public bool isPossessed = false;
    private ShooterEnemyBecomesPlayerController playerController;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        playerController = GetComponent<ShooterEnemyBecomesPlayerController>();
        gun = GetComponentInChildren<Gun>();

        if(gun == null)
        {
            Debug.LogError("No Gun component found in children!");
        }
    }

    void Start()
    {
        gameManager = GameManager.Instance;
    }

    void Update()
    {
        if (isPossessed)
        {
            HandlePossession();
            return;
        }

        UpdatePlayerReference();
        
        if(player == null) return;
        
        TrackPlayer();
        TryToShoot();
    }

    private void UpdatePlayerReference()
    {
        if (gameManager == null || gameManager.GetCurrentPossessedEntity() == null)
        {
            if (gameManager?.currentGhost != null)
            {
                player = gameManager.currentGhost;
            }
            else
            {
                return;
            }
        }
        else
        {
            player = gameManager.GetCurrentPossessedEntity();
        }
    }

    private void HandlePossession()
    {
        // Disable AI components when possessed
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        // Enable player controller
        if (playerController != null)
        {
            playerController.enabled = true;
        }

        // Disable this script's behavior
        enabled = false;
    }

    void TrackPlayer()
    {
        if (player == null || weaponHolder == null) return;

        // Body rotation
        Vector3 lookPos = player.transform.position - transform.position;
        Vector3 flatLookPos = new Vector3(lookPos.x, 0f, lookPos.z);
        transform.rotation = Quaternion.LookRotation(flatLookPos);

        // Weapon aiming
        Vector3 weaponToPlayer = player.transform.position - weaponHolder.position;
        float angle = -Mathf.Atan2(weaponToPlayer.y, weaponToPlayer.magnitude) * Mathf.Rad2Deg;
        weaponHolder.localRotation = Quaternion.Euler(angle, 0f, 0f);

        // Debug visualization
        Debug.DrawLine(transform.position + Vector3.up * heightOffset, 
                     player.transform.position, 
                     IsPlayerInRange() ? Color.red : Color.gray);
    }
    
    void TryToShoot()
    {
        if(!isShooting && IsPlayerInRange() && !isPossessed)
        {
            Vector3 direction = (player.transform.position - transform.position).normalized;
            StartCoroutine(StartShooting(direction));
        }
    }

    bool IsPlayerInRange()
    {
        if(player == null) return false;
        return Vector3.Distance(transform.position, player.transform.position) <= attackRange;
    }

    IEnumerator StartShooting(Vector3 direction)
    {
        isShooting = true;
        gun.Shoot(direction);
        yield return new WaitForSeconds(attackCooldown);
        isShooting = false;
    }

    // Public method to handle possession state
    public void SetPossessed(bool possessed)
    {
        isPossessed = possessed;
        if (possessed)
        {
            HandlePossession();
        }
        else
        {
            // Reset when unpossessed
            if (agent != null)
            {
                agent.enabled = true;
                agent.isStopped = false;
            }
            enabled = true;
        }
    }
}
