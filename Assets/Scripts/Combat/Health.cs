using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    
    [Header("Enemy Configuration")]
    public bool isStaticEnemy = false; // Set this for shooter enemies in inspector
    
    [Header("Hit Response")]
    public float knockbackForce = 5f;
    public bool useHitStop = true;
    public float hitStopDuration = 0.1f;
    
    [Header("Visual Feedback")]
    public Material hitFlashMaterial;
    public float hitFlashDuration = 0.1f;
    private Material[] originalMaterials;
    private Renderer[] meshRenderers;
    private Rigidbody rb;

    [Header("Knockback Settings")]
    public float baseKnockbackForce = 5f;
    public float playerReceivedKnockback = 1f;     
    public float playerDealtKnockback = 1.5f;      
    public float enemyKnockbackMultiplier = 1f;    

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

        hitDirection = hitDirection.normalized;
        
        currentHealth = Mathf.Max(0, currentHealth - damage);
        onHealthChanged?.Invoke(currentHealth / maxHealth);
        onDamaged?.Invoke();

        float knockbackMultiplier = GetKnockbackMultiplier();
        
        if(!isStaticEnemy) // Only apply knockback to non-static enemies
        {
            StartCoroutine(HitResponse(hitPoint, hitDirection, knockbackMultiplier));
        }
        else
        {
            StartCoroutine(HitEffectRoutine(hitPoint, hitDirection));
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private float GetKnockbackMultiplier()
    {
        if(GetComponent<EnemyBecomesPlayerController>() != null)
        {
            return playerReceivedKnockback;
        }
        return isStaticEnemy ? 0f : enemyKnockbackMultiplier;
    }

    private IEnumerator HitResponse(Vector3 hitPoint, Vector3 hitDirection, float knockbackMultiplier)
    {
        // Apply knockback only if not static enemy
        if (rb != null && !isStaticEnemy)
        {
            rb.AddForce(hitDirection * baseKnockbackForce * knockbackMultiplier, ForceMode.Impulse);
        }

        yield return StartCoroutine(HitEffectRoutine(hitPoint, hitDirection));
    }

    private IEnumerator HitEffectRoutine(Vector3 hitPoint, Vector3 hitDirection)
    {
        try 
        {
            if (!useHitStop) yield break;
        
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0.1f;
        
            if (hitFlashMaterial != null && meshRenderers != null)
            {
                foreach (var renderer in meshRenderers)
                {
                    if (renderer != null)
                        renderer.material = hitFlashMaterial;
                }
            }
        
            yield return new WaitForSecondsRealtime(hitStopDuration);
        
            Time.timeScale = originalTimeScale;
        
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
            SceneManager.LoadScene("Death Screen");
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
