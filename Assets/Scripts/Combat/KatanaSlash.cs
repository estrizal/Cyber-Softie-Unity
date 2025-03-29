using UnityEngine;
using System.Collections.Generic;

public class KatanaSlash : MonoBehaviour
{
    [Header("Hit Detection")]
    public LayerMask targetLayers;
    public float damageAmount = 10f;
    public float hitBoxActiveTime = 0.1f;
    
    [Header("Visual Effects")]
    public GameObject slashEffectPrefab;
    public float slashDuration = 0.3f;
    public bool followSword = true;

    [Header("Attack Settings")] 
    public bool debugMode = false;

    [Header("Effect Pool Settings")]
    public int poolSize = 5;
    private Queue<GameObject> slashEffectPool;
    private Transform effectPoolParent;
    
    private BoxCollider hitBox;
    protected bool isActive = false;
    private Transform swordTransform;
    private bool isAttacking = false;
    private HashSet<int> hitTargets; // Track hit targets to prevent multiple hits

    private void Awake()
    {
        hitBox = GetComponent<BoxCollider>();
        swordTransform = transform;
        hitTargets = new HashSet<int>();
        
        if (hitBox)
        {
            hitBox.isTrigger = true;
            hitBox.enabled = false;
        }
        else
        {
            Debug.LogError("KatanaSlash requires a BoxCollider!");
        }

        // Initialize effect pool
        InitializeEffectPool();
    }

    private void InitializeEffectPool()
    {
        slashEffectPool = new Queue<GameObject>();
        effectPoolParent = new GameObject("SlashEffectPool").transform;
        effectPoolParent.parent = transform;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject effect = Instantiate(slashEffectPrefab, effectPoolParent);
            effect.SetActive(false);
            slashEffectPool.Enqueue(effect);
        }
    }

    public void ActivateSlash()
    {
        if (!isActive && !isAttacking)
        {
            hitTargets.Clear(); // Clear hit targets for new slash
            isAttacking = true;
            StartCoroutine(SlashRoutine());
        }
    }

    private System.Collections.IEnumerator SlashRoutine()
    {
        isActive = true;
        hitBox.enabled = true;
        
        // Spawn effect
        if (slashEffectPrefab != null)
        {
            SpawnSlashEffect();
        }
        
        yield return new WaitForSeconds(hitBoxActiveTime);
        
        // Cleanup
        hitBox.enabled = false;
        isActive = false;
        isAttacking = false;
        hitTargets.Clear();
    }

    private void SpawnSlashEffect()
    {
        if (slashEffectPool.Count == 0) return;

        GameObject slashEffect = slashEffectPool.Dequeue();
        slashEffect.transform.position = transform.position + new Vector3(0, 1f, 0);
        slashEffect.transform.rotation = transform.rotation;
        slashEffect.SetActive(true);

        if (followSword)
        {
            slashEffect.transform.parent = transform;
        }

        StartCoroutine(ReturnToPool(slashEffect));
    }

    private System.Collections.IEnumerator ReturnToPool(GameObject effect)
    {
        yield return new WaitForSeconds(slashDuration);
        
        effect.SetActive(false);
        effect.transform.parent = effectPoolParent;
        slashEffectPool.Enqueue(effect);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive || !isAttacking) return;

        // Prevent multiple hits on same target during one slash
        int targetId = other.gameObject.GetInstanceID();
        if (hitTargets.Contains(targetId)) return;

        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            ProcessHit(other);
            hitTargets.Add(targetId);
        }
    }

    private void ProcessHit(Collider other)
    {
        // Skip if already hit
        int hitID = other.gameObject.GetInstanceID();
        if (hitTargets.Contains(hitID)) return;

        // First check if we hit a shield
        if (other.gameObject.CompareTag("Shield"))
        {
            // Shield hit - block damage and add visual/audio feedback here
            hitTargets.Add(hitID);
            // TODO: Add shield hit effects/sound
            return;
        }

        // If not blocked, check for health component and apply damage
        Health targetHealth = other.GetComponent<Health>();
        if (targetHealth != null)
        {
            EnemyBecomesPlayerController playerController = other.GetComponent<EnemyBecomesPlayerController>();
            if (playerController != null && playerController.isBlocking)
            {
                // Double check if player is blocking in correct direction
                float dotProduct = Vector3.Dot(playerController.transform.forward, 
                    (transform.position - playerController.transform.position).normalized);
                if (dotProduct > 0.5f) // Blocking angle check
                {
                    // Successfully blocked
                    hitTargets.Add(hitID);
                    return;
                }
            }

            // Apply damage if not blocked
            targetHealth.TakeDamage(damageAmount, transform.position, transform.forward);
            hitTargets.Add(hitID);
        }
    }

    private void OnDrawGizmos()
    {
        if (!hitBox || !debugMode) return;
        
        Gizmos.color = isActive ? Color.red : Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(hitBox.center, hitBox.size);
        
        // Draw attack direction
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 2f);
    }

    private void OnDestroy()
    {
        if (effectPoolParent != null)
        {
            Destroy(effectPoolParent.gameObject);
        }
    }
}
