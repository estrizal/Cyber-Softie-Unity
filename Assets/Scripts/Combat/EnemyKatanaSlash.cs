using UnityEngine;
using System.Collections.Generic;

public class EnemyKatanaSlash : MonoBehaviour
{
    [Header("Hit Detection")]
    public LayerMask targetLayers;
    public float damageAmount = 10f;
    public float hitBoxActiveTime = 0.1f;
    
    [Header("Visual Effects")]
    public GameObject slashEffectPrefab;
    public float slashDuration = 0.3f;
    public bool followSword = true;
    public bool slashThroughGround = true;

    [Header("Effect Pool Settings")]
    public int poolSize = 5;
    private Queue<GameObject> slashEffectPool;
    private Transform effectPoolParent;
    
    private BoxCollider hitBox;
    protected bool isActive = false;
    private bool isAttacking = false;
    private HashSet<int> hitTargets;

    private void Awake()
    {
        hitBox = GetComponent<BoxCollider>();
        hitTargets = new HashSet<int>();
        
        if (hitBox)
        {
            hitBox.isTrigger = true;
            hitBox.enabled = false;
        }
        else
        {
            Debug.LogError("EnemyKatanaSlash requires a BoxCollider!");
        }

        InitializeEffectPool();
    }

    private void InitializeEffectPool()
    {
        slashEffectPool = new Queue<GameObject>();
        effectPoolParent = new GameObject("EnemySlashEffectPool").transform;
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
            hitTargets.Clear();
            isAttacking = true;
            StartCoroutine(SlashRoutine());
        }
    }

    private System.Collections.IEnumerator SlashRoutine()
    {
        isActive = true;
        hitBox.enabled = true;
        
        if (slashEffectPrefab != null)
        {
            SpawnSlashEffect();
        }
        
        yield return new WaitForSeconds(hitBoxActiveTime);
        
        hitBox.enabled = false;
        isActive = false;
        isAttacking = false;
        hitTargets.Clear();
    }

    private void SpawnSlashEffect()
    {
        if (slashEffectPool.Count == 0) return;

        GameObject slashEffect = slashEffectPool.Dequeue();
        slashEffect.transform.position = transform.position + new Vector3(0, 0.7f, 0);
        slashEffect.transform.rotation = transform.rotation;
        slashEffect.SetActive(true);

        if (followSword)
        {
            slashEffect.transform.parent = transform;
        }

        // Handle particle sorting
        if (slashThroughGround)
        {
            var particles = slashEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                var renderer = particles.GetComponent<ParticleSystemRenderer>();
                renderer.sortMode = ParticleSystemSortMode.Distance;
            }
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
        Vector3 hitPoint = other.ClosestPoint(transform.position);
        Vector3 hitDirection = (hitPoint - transform.position).normalized;
        hitDirection.y = 0;

        Health healthComponent = other.GetComponent<Health>();
        if (healthComponent != null)
        {
            healthComponent.TakeDamage(damageAmount, hitPoint, hitDirection);
        }
    }

    private void OnDestroy()
    {
        if (effectPoolParent != null)
        {
            Destroy(effectPoolParent.gameObject);
        }
    }
}
