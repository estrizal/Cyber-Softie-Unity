using UnityEngine;

public class Gun : MonoBehaviour
{
    public float damage = 10f;
    public float range = 100f;
    public Transform bulletExitPoint;
    public LayerMask damageableLayer;
    public void Shoot(Vector3 direction)
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(bulletExitPoint.position, direction, out raycastHit, range, layerMask:damageableLayer))
        {
            Debug.Log(raycastHit.collider.name);
            Health health = raycastHit.transform.GetComponent<Health>();
            health.TakeDamage(damage, raycastHit.point, raycastHit.normal);
        }
    }
}
