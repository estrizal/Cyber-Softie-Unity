using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Assertions.Must;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour
{
    public enum EnemyState { Patrolling, Aggressive, Searching }
    public EnemyState currentState = EnemyState.Patrolling;

    [Header("Patrolling Settings")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 3.5f;
    public float waitTimeAfterPatrol = 1.5f;

    [Header("Aggressive Settings")]
    private Transform playerTransform;
    public float chaseSpeed = 5f;
    public float chaseRange = 10f;

    [Header("Searching Settings")]
    public float searchDuration = 5f;
    private float searchTimer;

    [Header("Possession Settings")]
    public bool isPossessed = false;

    private NavMeshAgent agent;
    private int currentPatrolIndex;
    
    private bool isMoving = false;
    private bool isWaiting = false;
    private Animator animator;
    
    [Header("Attack Settings")]
    public float attackRange = 2f;
    private bool attacking = false;
    public float attackCooldown = 0.9f;
    public float attackDuration = 0.6f;  // Add this line - time for attack animation
    public float animationFactor = 0.6f;
    public float attackWindupTime = 0.4f;  // Time before attack starts
    private EnemyBecomesPlayerController enemyBecomesPlayerController;
    private GameManager gameManager;

    [Header("Vision Settings")]
    public float sightRange = 15f;
    public float fieldOfViewAngle = 100f;
    //public LayerMask obstacleLayer;
    
    [Header("Coordination Settings")]
    public float minDistanceToJoinChase = 20f;
    public float chanceToJoinChase = 0.3f;
    public float decisionUpdateInterval = 0.5f;
    private float nextDecisionTime;
    private bool canSeePlayer;
    
    private Vector3? lastKnownPlayerPosition;

    private Health health;

    [Header("Knockback Settings")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.3f;

    private bool isInCooldown = false;

    // Add component validation to Awake
    void Awake()
    {
        // Get required components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyBecomesPlayerController = GetComponent<EnemyBecomesPlayerController>();
        health = GetComponent<Health>();

        // Validate required components
        if (agent == null) Debug.LogError($"Missing NavMeshAgent on {gameObject.name}");
        if (animator == null) Debug.LogError($"Missing Animator on {gameObject.name}");
        if (enemyBecomesPlayerController == null) Debug.LogError($"Missing EnemyBecomesPlayerController on {gameObject.name}");
        if (health == null) Debug.LogError($"Missing Health on {gameObject.name}");

        // Initialize variables
        currentPatrolIndex = 0;
        searchTimer = searchDuration;
        nextDecisionTime = Time.time + Random.Range(0f, decisionUpdateInterval);

        // Set up health listeners
        if (health != null)
        {
            health.onDeath.AddListener(HandleDeath);
            health.onDamaged.AddListener(HandleDamaged);
        }
    }

    void Start()
    {
        gameManager = GameManager.Instance;
    } 

    void Update()
    {
        if (gameManager == null || gameManager.GetCurrentPossessedEntity() == null)
        {
            
            
            // Get player position from ghost if not possessed
            if (gameManager?.currentGhost != null)
            {
                playerTransform = gameManager.currentGhost.transform;
            }
            else
            {
                return;
            }
        }
        else
        {
            playerTransform = gameManager.GetCurrentPossessedEntity().transform;
        }

        if (isPossessed)
        {
            PossessedBehavior();
            return;
        }

        if (Time.time >= nextDecisionTime)
        {
            canSeePlayer = CheckIfCanSeePlayer();
            nextDecisionTime = Time.time + decisionUpdateInterval;
        }

        switch (currentState)
        {
            case EnemyState.Patrolling:
                Patrol();
                CheckForThreat();
                break;
            case EnemyState.Aggressive:
                ChasePlayer();
                break;
            case EnemyState.Searching:
                SearchForPlayer();
                break;
        }

        UpdateMovementState();
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;
        
        if (!isWaiting && agent.destination != patrolPoints[currentPatrolIndex].position)
        {
            agent.destination = patrolPoints[currentPatrolIndex].position;
        }

        if (!isWaiting && !agent.pathPending && agent.remainingDistance < 0.5f)
        {
            StartCoroutine(WaitAfterPatrol());
        }
    }

    IEnumerator WaitAfterPatrol()
    {
        isWaiting = true;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            
            agent.isStopped = true;
        }
        
        yield return new WaitForSeconds(Random.Range(waitTimeAfterPatrol, waitTimeAfterPatrol+1.5f));
        
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        isWaiting = false;
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    private void ChasePlayer()
    {
        // Early exit if agent is not valid or in cooldown
        if (!agent || !agent.enabled || !agent.isOnNavMesh || isInCooldown) 
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Update last known position if player is visible
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }

        // In attack range - Stop and attack
        if (distanceToPlayer <= attackRange)
        {
            // Stop movement
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            // Always face the player
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);

            // Attack if not on cooldown
            if (!attacking && !isInCooldown)
            {
                StartCoroutine(StartAttack());
            }
        }
        // Chase range but outside attack range - Use coordinated movement
        else if (distanceToPlayer <= chaseRange)
        {
            HandleCoordinatedMovement(distanceToPlayer);
        }
        // Lost player - Switch to searching
        else if (!canSeePlayer)
        {
            currentState = EnemyState.Searching;
            searchTimer = searchDuration;
        }
    }

    private void TryRecoverAgent()
    {
        if (agent == null) return;

        // Try to recover agent state
        StartCoroutine(RecoverAgentRoutine());
    }

    private IEnumerator RecoverAgentRoutine()
    {
        // Disable agent temporarily
        agent.enabled = false;
        
        // Wait a frame
        yield return null;
        
        // Try to find valid position on NavMesh
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
        {
            transform.position = hit.position;
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    private void HandleCoordinatedMovement(float distanceToPlayer)
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh) return;

        // If too far from player, transition to normal chase
        if (distanceToPlayer > attackRange * 3)
        {
            agent.speed = chaseSpeed;
            agent.destination = playerTransform.position;
            return;
        }

        try
        {
            // Calculate flanking position
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up);
            float randomOffset = Mathf.PerlinNoise(Time.time * 0.5f, gameObject.GetInstanceID() * 0.5f) * 2 - 1;
            Vector3 flankPosition = playerTransform.position + (perpendicular * attackRange * 1.5f * randomOffset);
            
            // Validate position on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(flankPosition, out hit, attackRange * 1.5f, NavMesh.AllAreas))
            {
                agent.stoppingDistance = attackRange * 0.8f;
                agent.speed = chaseSpeed * 0.8f; // Slightly slower for better positioning
                agent.destination = hit.position;
            }
            else
            {
                // Fallback to direct chase if no valid flank position
                agent.stoppingDistance = attackRange * 0.8f;
                agent.speed = chaseSpeed;
                agent.destination = playerTransform.position;
            }
        }
        catch (System.Exception)
        {
            // Fallback behavior if anything goes wrong
            if (agent && agent.enabled && agent.isOnNavMesh)
            {
                agent.stoppingDistance = attackRange * 0.8f;
                agent.speed = chaseSpeed;
                agent.destination = playerTransform.position;
            }
        }
    }

    private void SearchForPlayer()
    {
        if (canSeePlayer)
        {
            currentState = EnemyState.Aggressive;
            return;
        }

        searchTimer -= Time.deltaTime;
        if (searchTimer <= 0)
        {
            currentState = EnemyState.Patrolling;
            return;
        }

        if (!agent.hasPath || agent.remainingDistance < 0.5f)
        {
            float searchChoice = Random.value;
            Vector3 targetPosition;

            if (searchChoice < 0.4f)
            {
                Vector3 searchCenter = lastKnownPlayerPosition ?? transform.position;
                Vector3 randomDirection = Random.insideUnitSphere * (chaseRange * 0.5f);
                randomDirection.y = 0;
                targetPosition = searchCenter + randomDirection;
            }
            else if (searchChoice < 0.7f)
            {
                Vector3 randomDirection = Random.insideUnitSphere.normalized * chaseRange;
                randomDirection.y = 0;
                targetPosition = transform.position + randomDirection;
            }
            else
            {
                StartCoroutine(PauseAndLook());
                return;
            }

            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, chaseRange, NavMesh.AllAreas))
            {
                agent.speed = patrolSpeed;
                agent.isStopped = false;
                agent.destination = hit.position;
            }
        }
    }

    // Fix PauseAndLook coroutine syntax
    private IEnumerator PauseAndLook()
    {
        agent.isStopped = true;
        isMoving = false;

        float originalYRotation = transform.eulerAngles.y;
        float[] lookDirections = { -60f, 60f, -120f, 120f };

        foreach (float angle in lookDirections)
        {
            float targetRotation = originalYRotation + angle;
            float elapsedTime = 0f;
            float waitTime = Random.Range(0.5f, 1f);

            while (elapsedTime < waitTime)
            {
                float currentYRotation = Mathf.LerpAngle(transform.eulerAngles.y, targetRotation, elapsedTime / waitTime);
                transform.eulerAngles = new Vector3(0, currentYRotation, 0);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        transform.eulerAngles = new Vector3(0, originalYRotation, 0);
        agent.isStopped = false;
    }

    private IEnumerator StartAttack()
    {
        if (!agent.enabled || !agent.isOnNavMesh) yield break;
        
        attacking = true;
         // Stop movement and face player during windup
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Windup phase - enemy stands still and prepares
        float elapsedTime = 0f;
        while (elapsedTime < attackWindupTime)
    {
        // Keep facing the player during windup
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = playerTransform.position - transform.position;
            directionToPlayer.y = 0;
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
        elapsedTime += Time.deltaTime;
        yield return null;
    }
        // Stop movement for attack
        EnemyKatanaSlash enemyKatanaSlash = GetComponentInChildren<EnemyKatanaSlash>();
        if (enemyKatanaSlash != null)
        {
            enemyKatanaSlash.ActivateSlash();
        }
        animator.SetTrigger("Attack");
        // Wait for attack animation to complete
        yield return new WaitForSeconds(attackDuration);
        
        attacking = false;
        
        // Start cooldown state
        StartCoroutine(HandleAttackCooldown());
    }

    private IEnumerator HandleAttackCooldown()
    {
        isInCooldown = true;
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            
            agent.isStopped = true;
        }
        
        yield return new WaitForSeconds(attackCooldown);
        
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }
        isInCooldown = false;
    }

    private void PossessedBehavior()
    {
       
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        if (enemyBecomesPlayerController != null)
        {
            enemyBecomesPlayerController.enabled = true;
        }
    }

    private bool CheckIfCanSeePlayer()
    {
        if (playerTransform == null) return false;

        Vector3 directionToPlayer = playerTransform.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > sightRange) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > fieldOfViewAngle * 0.5f)
        {
            if (distanceToPlayer <= sightRange * 0.3f) 
            {
                return true;
            }
            return false;
        }

        RaycastHit hit;
        Vector3 eyePosition = transform.position + Vector3.up;
        if (Physics.Raycast(eyePosition, directionToPlayer.normalized, out hit, sightRange))
        {
            return hit.transform == playerTransform;
        }

        return true;
    }

    private void CheckForThreat()
    {
        if (canSeePlayer)
        {
            currentState = EnemyState.Aggressive;
            lastKnownPlayerPosition = playerTransform.position;
            
            AlertNearbyEnemies();
            return;
        }

        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, minDistanceToJoinChase);
        bool nearbyEnemyChasing = nearbyColliders
            .Select(c => c.GetComponent<EnemyController>())
            .Where(e => e != null && e != this)
            .Any(e => e.currentState == EnemyState.Aggressive);

        if (nearbyEnemyChasing && Random.value < chanceToJoinChase)
        {
            currentState = EnemyState.Aggressive;
        }
    }

    private void AlertNearbyEnemies()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, minDistanceToJoinChase);
        foreach (var collider in nearbyColliders)
        {
            EnemyController nearbyEnemy = collider.GetComponent<EnemyController>();
            if (nearbyEnemy != null && nearbyEnemy != this && nearbyEnemy.currentState == EnemyState.Patrolling)
            {
                nearbyEnemy.currentState = EnemyState.Aggressive;
            }
        }
    }

    private void HandleDeath()
    {
        // Stop all coroutines to prevent lingering NavMesh calls
        StopAllCoroutines();
        
        // Disable agent before animations
        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }
        
        enabled = false;
        animator.Play("Death");
    }

    private void HandleDamaged()
    {
        animator.Play("Hit");
        
        if (agent != null)
        {
            StartCoroutine(HandleKnockback());
        }
    }

    private IEnumerator HandleKnockback()
    {
        float timer = 0f;
        Vector3 knockbackDirection;

        // Get direction from the appropriate attacker (ghost or possessed enemy)
        if (GameManager.Instance != null)
        {
            Transform attackerTransform = null;
            
            // Check if there's a possessed entity attacking
            GameObject possessedEntity = GameManager.Instance.GetCurrentPossessedEntity();
            if (possessedEntity != null && possessedEntity != gameObject)
            {
                attackerTransform = possessedEntity.transform;
            }
            // Otherwise use ghost as attacker
            else if (GameManager.Instance.currentGhost != null)
            {
                attackerTransform = GameManager.Instance.currentGhost.transform;
            }

            // Calculate knockback direction if we have an attacker
            if (attackerTransform != null)
            {
                knockbackDirection = transform.position - attackerTransform.position;
                knockbackDirection.y = 0;
                knockbackDirection.Normalize();

                // Store original NavMeshAgent settings
                bool wasAgentEnabled = agent != null && agent.enabled;
                float originalSpeed = agent != null ? agent.speed : 0f;

                // Safely disable agent for knockback
                if (agent != null && agent.enabled)
                {
                    agent.enabled = false;
                }

                // Apply knockback movement
                while (timer < knockbackDuration)
                {
                    float knockbackSpeed = Mathf.Lerp(knockbackForce, 0f, timer / knockbackDuration);
                    transform.position += knockbackDirection * knockbackSpeed * Time.deltaTime;
                    timer += Time.deltaTime;
                    yield return null;
                }

                // Validate position and re-enable agent
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 1.0f, NavMesh.AllAreas))
                {
                    transform.position = hit.position;
                    
                    // Only re-enable agent if it was enabled before
                    if (wasAgentEnabled && agent != null)
                    {
                        agent.enabled = true;
                        agent.speed = originalSpeed;
                    }
                }
            }
        }
    }

    // Fix UpdateMovementState method
    private void UpdateMovementState()
    {
        if (!agent || !agent.enabled || !agent.isOnNavMesh) return;

        // Update movement state based on agent velocity
        isMoving = agent.velocity.magnitude > 0.2f;
        animator.SetBool("isMoving", isMoving);
        
        if (isMoving)
        {
            animator.SetFloat("Speed", agent.velocity.magnitude/chaseSpeed * animationFactor);
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }
}
