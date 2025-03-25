using UnityEngine;

public class EnemyKatanaSlash : KatanaSlash
{
    [Header("Slash Settings")]
    public bool slashThroughGround = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!isActive) return;
        
        // Check if we hit the player
        EnemyBecomesPlayerController player = other.GetComponent<EnemyBecomesPlayerController>();
        if (player != null)
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            Vector3 hitDirection = (hitPoint - transform.position).normalized;
            hitDirection.y = 0; // Keep slash effect parallel to ground
            
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount, hitPoint, hitDirection);
                SpawnSlashEffect(hitPoint, hitDirection);
            }
        }
    }

    private void SpawnSlashEffect(Vector3 hitPoint, Vector3 hitDirection)
    {
        if (slashEffectPrefab != null)
        {
            // Spawn effect at hit point with correct rotation
            Quaternion hitRotation = Quaternion.LookRotation(hitDirection);
            GameObject slashEffect = Instantiate(slashEffectPrefab, hitPoint, hitRotation);
            
            // Only handle sorting for isometric view
            if (slashThroughGround)
            {
                var particles = slashEffect.GetComponent<ParticleSystem>();
                if (particles != null)
                {
                    var renderer = particles.GetComponent<ParticleSystemRenderer>();
                    renderer.sortMode = ParticleSystemSortMode.Distance;
                }
            }
            
            Destroy(slashEffect, slashDuration);
        }
    }
}