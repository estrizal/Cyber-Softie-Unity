using UnityEngine;

public class ISOshift : MonoBehaviour
{

    public string triggeringTag = "Player"; // Tag of the object that triggers rotation
    private Collider iso_collider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        iso_collider = GetComponent<Collider>();
        if (iso_collider == null)
        {
            Debug.LogError("RotatingVent: No Collider found on this GameObject.");
        }
    }

    // Update is called once per frame


    private void OnTriggerEnter(Collider other)
    {
        if (!iso_collider.enabled || !other.CompareTag(triggeringTag)) return;

        Debug.Log($"Bhai sahab... Waaowwwww");

        iso_collider.enabled = false;

        // Set new target rotation based on electricity status

        if (GameManager.Instance != null)
        {
            if (other.gameObject == GameManager.Instance.currentGhost ||
                other.gameObject == GameManager.Instance.GetCurrentPossessedEntity())
            {
                GameManager.Instance.SwitchToIsometricCamera();
                Debug.Log($"Camera mode trigger activated by {other.gameObject.name}");
            }




        }

    }

}