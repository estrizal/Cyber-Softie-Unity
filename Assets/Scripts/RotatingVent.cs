using UnityEngine;
using System;
using System.Collections;

public class RotatingVent : MonoBehaviour
{
    [Header("Rotation Settings")]
    public float rotationSpeed = 90f; // Degrees per second for smooth rotation
    private Quaternion targetRotation;
    private bool shouldRotate = false;
    private bool isElectricityFixed = false;
    public float triggerCooldown = 2f;


    [Header("Trigger Settings")]
    public string triggeringTag = "Player"; // Tag of the object that triggers rotation
    private Collider ventTriggerCollider; // Reference to the vent's trigger collider

    // Event to notify other systems when the vent is triggered
    public static event Action OnVentTriggered;
    public AudioSource Vent_Opening_Sound;
    public AudioSource Fan;

    void OnEnable()
    {
        FixElectricity.OnElectricityFixed += ElectricityFixed;
    }

    void OnDisable()
    {
        FixElectricity.OnElectricityFixed -= ElectricityFixed;
    }

    void Start()
    {
        ventTriggerCollider = GetComponent<Collider>();
        if (ventTriggerCollider == null)
        {
            Debug.LogError("RotatingVent: No Collider found on this GameObject.");
        }
    }

    void Update()
    {
        if (shouldRotate)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                transform.rotation = targetRotation;
                shouldRotate = false; // Stop rotating when done

                // Disable the trigger collider after rotating

                // Notify other systems that the vent has finished rotating
                OnVentTriggered?.Invoke();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!ventTriggerCollider.enabled || !other.CompareTag(triggeringTag)) return;

        Debug.Log($"Rotating Vent triggered by {other.gameObject.name}!");

        // Set new target rotation based on electricity status
        Vector3 currentEulerAngles = transform.rotation.eulerAngles;


        float rotationAngle;
        if (isElectricityFixed == true)
        {
            rotationAngle = -85f;
            ventTriggerCollider.enabled = false;
            Debug.Log("Vent trigger collider disabled.");
            if (Vent_Opening_Sound != null){
                Vent_Opening_Sound.Play();
            }

            if (Fan != null)
            {
                Fan.Stop();        
            }
        
        }

        else {
            rotationAngle = 85f;
            if (Vent_Opening_Sound != null)
            {
                Vent_Opening_Sound.Play();
                StartCoroutine(DisableTriggerTemporarily());
            }
        }
        //float rotationAngle = isElectricityFixed ? -85f : 85f; // +85 if fixed, -85 otherwise
        targetRotation = Quaternion.Euler(currentEulerAngles.x + rotationAngle, currentEulerAngles.y, currentEulerAngles.z);

        shouldRotate = true;

        // Optional: Add sound or animation feedback here
    }





    private IEnumerator DisableTriggerTemporarily()
    {
        ventTriggerCollider.enabled = false; // Disable trigger collider
        Debug.Log("Vent trigger collider disabled temporarily.");

        yield return new WaitForSeconds(triggerCooldown); // Wait for cooldown duration

        ventTriggerCollider.enabled = true; // Re-enable trigger collider
        Debug.Log("Vent trigger collider re-enabled.");
    }




    private void ElectricityFixed()
    {
        isElectricityFixed = true;
        Debug.Log("Electricity fixed: Vent will now rotate +85 degrees when triggered.");
    }
}
