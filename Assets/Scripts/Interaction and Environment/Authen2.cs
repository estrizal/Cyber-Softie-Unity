using UnityEngine;

public class Authen2 : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public string MESSAGE = "Press and hold 'E' to hack"; // Default message
    public string InteractionPrompt => isElectricityFixed ? MESSAGE : "Electricity is not fixed!"; // Dynamic prompt based on electricity status

    public AudioSource Authen_Success;
    public AudioSource Authen_Failed;

    [Header("References")]
    public Gate gateToOpen; // Reference to the gate object

    private bool isActivated = false;
    private bool isElectricityFixed = false; // Tracks whether electricity is fixed

    void OnEnable()
    {
        FixElectricity.OnElectricityFixed += ElectricityFixed; // Subscribe to electricity fix event
    }

    void OnDisable()
    {
        FixElectricity.OnElectricityFixed -= ElectricityFixed; // Unsubscribe from event
    }

    public void Interact()
    {
        if (!isElectricityFixed)
        {
            Debug.Log("Cannot hack: Electricity is not fixed!");

            if (Authen_Failed != null )
            {
                Authen_Failed.Play();
            }

            return; // Do nothing if electricity is not fixed
        }

        if (isActivated) return;

        isActivated = true;
        gateToOpen.OpenGate(); // Open the gate
        Debug.Log("Authentication successful. Gate opening...");
        if (Authen_Success != null)
        {
            Authen_Success.Play();
        }
        // Optional: Add sound or animation feedback here
    }

    private void ElectricityFixed()
    {
        isElectricityFixed = true;
        Debug.Log("Electricity has been fixed: Authentication button now functional.");
    }
}
