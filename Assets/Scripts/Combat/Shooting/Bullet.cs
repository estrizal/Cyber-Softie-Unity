using UnityEngine;
public class Bullet : MonoBehaviour
{
    public float damage = 10f;
    
    private void OnCollisionEnter(Collision collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        if(health != null)
        {
            health.TakeDamage(damage, collision.contacts[0].point, collision.contacts[0].normal);
        }
        
        // Immediately return to pool on impact
        gameObject.SetActive(false);
    }
}

