using System.Collections;
using UnityEngine;

public class ShooterEnemyCombat : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject player;
    private Gun gun;
    public float attackCooldown = 1f;
    private bool isShooting;
    public Transform weaponHolder;

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
            // Get player position from ghost if not possessed
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
        TryToShoot();
        TrackPlayer();
    }

    void TrackPlayer()
    {
        if (player == null || weaponHolder == null) return;

        // Body rotation (left-right)
        Vector3 lookPos = player.transform.position - transform.position;
        Vector3 flatLookPos = new Vector3(lookPos.x, 0f, lookPos.z);
        transform.rotation = Quaternion.LookRotation(flatLookPos);

        // Weapon holder rotation (up-down)
        Vector3 weaponToPlayer = player.transform.position - weaponHolder.position;
        float angle = -Mathf.Atan2(weaponToPlayer.y, weaponToPlayer.magnitude) * Mathf.Rad2Deg;
        weaponHolder.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
    
    void TryToShoot()
    {
        if(!isShooting)
        {
            Vector3 direction = player.transform.position - transform.position;
            StartCoroutine(StartShooting(direction));
        }
    }

    IEnumerator StartShooting(Vector3 direction)
    {
        isShooting = true;
        gun.Shoot(direction);
        yield return new WaitForSeconds(attackCooldown);
        isShooting = false;
    }

}

