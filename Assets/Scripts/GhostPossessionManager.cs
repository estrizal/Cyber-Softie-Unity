using UnityEngine;
using UnityEngine.AI;

public class GhostPossessionManager : MonoBehaviour
{
    [Header("Possession Settings")]
    public LayerMask possessableLayer;
    public float possessionRange = 2f;

    private InputReader inputReader;
    private GameObject currentPossessedEntity;
    private GhostCharacterController ghostController;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        ghostController = GetComponent<GhostCharacterController>();

        if (inputReader != null)
        {
            inputReader.OnInteractPerformed += TryPossessNearbyEnemy;
            inputReader.OnInteract2Performed += HandleDepossession;
        }
    }

    private void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnInteractPerformed -= TryPossessNearbyEnemy;
            inputReader.OnInteract2Performed -= HandleDepossession;
        }
    }

    private void TryPossessNearbyEnemy()
    {
        Collider[] nearbyEntities = Physics.OverlapSphere(transform.position, possessionRange, possessableLayer);

        foreach (var col in nearbyEntities)
        {
            var katanaEnemy = col.GetComponent<EnemyController>();
            var shooterEnemy = col.GetComponent<ShooterEnemyCombat>();

            if (katanaEnemy != null && !katanaEnemy.isPossessed)
            {
                PossessEntity(col.gameObject);
                break;
            }
            else if (shooterEnemy != null && !shooterEnemy.isPossessed)
            {
                PossessEntity(col.gameObject);
                break;
            }
        }
    }

    private void PossessEntity(GameObject entity)
    {
        if (currentPossessedEntity != null)
        {
            HandleDepossession();
        }

        currentPossessedEntity = entity;

        var enemyController = entity.GetComponent<EnemyController>();
        var shooterCombat = entity.GetComponent<ShooterEnemyCombat>();
        var possessedController = entity.GetComponent<EnemyBecomesPlayerController>();
        var shooterController = entity.GetComponent<ShooterEnemyBecomesPlayerController>();
        var agent = entity.GetComponent<NavMeshAgent>();
        var input = entity.GetComponent<InputReader>();
        var health = entity.GetComponent<Health>();

        if (enemyController) { enemyController.isPossessed = true; enemyController.enabled = false; }
        if (shooterCombat) shooterCombat.enabled = false;
        if (agent) agent.enabled = false;

        if (possessedController)
        {
            possessedController.enabled = true;
            possessedController.isPossessed = true;
        }

        if (shooterController)
        {
            shooterController.enabled = true;
            shooterController.isPossessed = true;
        }

        if (input != null)
        {
            input.ResetInput();
            input.OnInteract2Performed += HandleDepossession;
            input.enabled = true;
        }

        if (health != null)
        {
            health.maxHealth = 100f;
            health.currentHealth = 100f;
        }

        entity.tag = "Player";
        gameObject.SetActive(false); // disable ghost
        if (ghostController) ghostController.enabled = false;
    }

    private void HandleDepossession()
    {
        if (currentPossessedEntity == null) return;

        var enemyForward = currentPossessedEntity.transform.forward;
        Vector3 spawnPos = currentPossessedEntity.transform.position - enemyForward * 2f;
        spawnPos.y += 0.5f;

        var enemyController = currentPossessedEntity.GetComponent<EnemyController>();
        var shooterCombat = currentPossessedEntity.GetComponent<ShooterEnemyCombat>();
        var input = currentPossessedEntity.GetComponent<InputReader>();
        var agent = currentPossessedEntity.GetComponent<NavMeshAgent>();
        var health = currentPossessedEntity.GetComponent<Health>();
        var possessedController = currentPossessedEntity.GetComponent<EnemyBecomesPlayerController>();
        var shooterController = currentPossessedEntity.GetComponent<ShooterEnemyBecomesPlayerController>();

        if (enemyController) { enemyController.isPossessed = false; enemyController.enabled = true; }
        if (shooterCombat) shooterCombat.enabled = true;
        if (agent) agent.enabled = true;
        if (possessedController) { possessedController.enabled = false; possessedController.isPossessed = false; }
        if (shooterController) { shooterController.enabled = false; shooterController.isPossessed = false; }

        if (input != null)
        {
            input.OnInteract2Performed -= HandleDepossession;
            input.enabled = false;
        }

        if (health != null)
        {
            health.maxHealth = 30f;
            health.currentHealth = 30f;
        }

        transform.position = spawnPos;
        gameObject.SetActive(true);
        if (ghostController) ghostController.enabled = true;

        currentPossessedEntity.tag = "Untagged";
        currentPossessedEntity = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, possessionRange);
    }
}
