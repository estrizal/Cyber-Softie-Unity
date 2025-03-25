using UnityEngine;

public class RealWindTunnel : MonoBehaviour
{
    [Header("Wind Tunnel Settings")]
    public float upwardForce = 10f; // Force applied to the player
    private bool isActive = false; // Determines if the wind tunnel is active

    void OnEnable()
    {
        FixElectricity.OnElectricityFixed += ActivateWindTunnel;
    }

    void OnDisable()
    {
        FixElectricity.OnElectricityFixed -= ActivateWindTunnel;
    }

    void OnTriggerStay(Collider other)
    {
        if (!isActive) return; // Only apply force if electricity is fixed

        Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
        if (playerRigidbody != null && other.CompareTag("Player"))
        {
            // Apply upward force to the player's Rigidbody
            playerRigidbody.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);
            Debug.Log("Player is being pushed up by the wind tunnel.");
        }
    }

    private void ActivateWindTunnel()
    {
        isActive = true;
        Debug.Log("Wind Tunnel activated after electricity was fixed.");
    }
}
