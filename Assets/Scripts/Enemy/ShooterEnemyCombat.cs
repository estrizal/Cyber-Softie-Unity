using System.Collections;
using UnityEngine;

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
    private const float heightOffset = 0.5f; // To account for character height differences

    void Start()
    {
        gameManager = GameManager.Instance;
        gun = GetComponentInChildren<Gun>();
        if(gun == null)
        {
            Debug.LogError("No Gun component found in children!");
        }
    }

    void Update()
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
        
        if(player == null) return;
        
        TrackPlayer();
        TryToShoot();
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
        if(!isShooting && IsPlayerInRange())
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
}
