using UnityEngine;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public int poolSize = 20;
    public float bulletSpeed = 30f;
    public float bulletLifetime = 3f;

    [Header("References")]
    public Transform bulletExitPoint;

    private Queue<GameObject> bulletPool;

    private void Awake()
    {
        InitializePool();
    }

    void InitializePool()
    {
        bulletPool = new Queue<GameObject>();
        
        for(int i = 0; i < poolSize; i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletPool.Enqueue(bullet);
        }
    }

    public void Shoot(Vector3 direction)
    {
        GameObject bullet = GetPooledBullet();
        
        if(bullet != null)
        {
            // Position and activate bullet
            bullet.transform.position = bulletExitPoint.position;
            bullet.transform.rotation = Quaternion.LookRotation(direction);
            bullet.SetActive(true);
            
            // Apply physics force
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.linearVelocity = Vector3.zero; // Reset velocity
            rb.AddForce(direction.normalized * bulletSpeed, ForceMode.VelocityChange);
            
            // Return to pool after lifetime expires
            StartCoroutine(ReturnBulletToPool(bullet, bulletLifetime));
        }
    }

    private GameObject GetPooledBullet()
    {
        // Check for available bullets in pool
        foreach(GameObject bullet in bulletPool)
        {
            if(!bullet.activeInHierarchy)
            {
                return bullet;
            }
        }
        
        // Optional: Expand pool if empty (remove if you want fixed size)
        // GameObject newBullet = Instantiate(bulletPrefab);
        // bulletPool.Enqueue(newBullet);
        // return newBullet;
        
        Debug.LogWarning("Bullet pool exhausted!");
        return null;
    }

    private System.Collections.IEnumerator ReturnBulletToPool(GameObject bullet, float delay)
    {
        yield return new WaitForSeconds(delay);
    
        if(bullet.activeInHierarchy)
        {
            BulletTrail bulletTrail = bullet.GetComponent<BulletTrail>();
            if(bulletTrail != null)
            {
                bulletTrail.ResetTrail();
            }
        
            bullet.SetActive(false);
            bullet.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            bulletPool.Enqueue(bullet);
        }
    }
}
