using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Hit Response")]
    public float knockbackForce = 5f;
    public bool useHitStop = true;
    public float hitStopDuration = 0.1f;
    
    [Header("Visual Feedback")]
    public GameObject slashEffectPrefab;
    public Material hitFlashMaterial;
    public float hitFlashDuration = 0.1f;
    private Material[] originalMaterials;
    private Renderer[] meshRenderers;
    private Rigidbody rb;

    [Header("Knockback Settings")]
    public float baseKnockbackForce = 5f;
    public float playerReceivedKnockback = 1f;     // Player takes normal knockback
    public float playerDealtKnockback = 1.5f;      // Player deals more knockback
    public float enemyKnockbackMultiplier = 1f;    // Base multiplier for enemies

    public UnityEvent onDeath;
    public UnityEvent<float> onHealthChanged;
    public UnityEvent onDamaged;

    private void Awake()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();
        meshRenderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[meshRenderers.Length];
        for (int i = 0; i < meshRenderers.Length; i++)
        {
            originalMaterials[i] = meshRenderers[i].material;
        }
    }

    public void TakeDamage(float damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        if (currentHealth <= 0 || damage <= 0) return;

        // Ensure hit direction is normalized
        hitDirection = hitDirection.normalized;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth / maxHealth);
        onDamaged?.Invoke();

        float knockbackMultiplier = GetComponent<EnemyBecomesPlayerController>() != null 
            ? playerReceivedKnockback 
            : playerDealtKnockback;

        StartCoroutine(HitResponse(hitPoint, hitDirection, knockbackMultiplier));

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private IEnumerator HitResponse(Vector3 hitPoint, Vector3 hitDirection, float knockbackMultiplier)
    {
        // Apply knockback
        if (rb != null)
        {
            rb.AddForce(hitDirection * baseKnockbackForce * knockbackMultiplier, ForceMode.Impulse);
        }

        // Spawn slash effect
        if (slashEffectPrefab != null)
        {
            Quaternion hitRotation = Quaternion.LookRotation(hitDirection);
            Instantiate(slashEffectPrefab, hitPoint, hitRotation);
        }

        // Hit stop and material flash
        yield return StartCoroutine(HitEffectRoutine(hitPoint, hitDirection));
    }

    private IEnumerator HitEffectRoutine(Vector3 hitPoint, Vector3 hitDirection)
    {
        try 
        {
            if (!useHitStop) yield break;
        
            // Store original time scale
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0.1f;
        
            // Handle hit flash materials
            if (hitFlashMaterial != null && meshRenderers != null)
            {
                foreach (var renderer in meshRenderers)
                {
                    if (renderer != null)
                        renderer.material = hitFlashMaterial;
                }
            }
        
            yield return new WaitForSecondsRealtime(hitStopDuration);
        
            // Always reset time scale
            Time.timeScale = originalTimeScale;
        
            // Reset materials only if object still exists
            if (this != null && meshRenderers != null)
            {
                for (int i = 0; i < meshRenderers.Length; i++)
                {
                    if (meshRenderers[i] != null && originalMaterials[i] != null)
                        meshRenderers[i].material = originalMaterials[i];
                }
            }
        }
        finally
        {
            // Guarantee time scale reset even if coroutine is interrupted
            if (Time.timeScale < 1f)
            {
                Time.timeScale = 1f;
            }
        }
    }

    private void Die()
    {
        onDeath?.Invoke();
        
        if (GetComponent<EnemyBecomesPlayerController>() != null)
        {
            // Player death
            gameObject.SetActive(false);
        }
        else
        {
            // Enemy death
            Destroy(gameObject, 0.5f);
        }
    }

    private void OnDisable()
    {
        // Ensure time scale reset when object is disabled
        if (Time.timeScale < 1f)
        {
            Time.timeScale = 1f;
        }
    }

    private void OnDestroy()
    {
        // Final safety check for time scale
        if (Time.timeScale < 1f)
        {
            Time.timeScale = 1f;
        }
    }
}