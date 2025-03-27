using UnityEngine;
using System.Collections;

public class Authen2 : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public string MESSAGE = "Press and hold 'E' to hack"; // Default message
    public string InteractionPrompt => isElectricityFixed ? MESSAGE : "Electricity is not fixed!"; // Dynamic prompt based on electricity status

    public AudioSource Authen_Success;
    public AudioSource Authen_Failed;
    public AudioSource combat_song;
    public AudioSource Voiceover2;
    public AudioSource bg_song;

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
            bg_song.Stop();
            Authen_Success.Play();

            StartCoroutine(PlayDelayedVoiceover());
        }
        // Optional: Add sound or animation feedback here
    }

    private void ElectricityFixed()
    {
        isElectricityFixed = true;
        Debug.Log("Electricity has been fixed: Authentication button now functional.");
    }

    private IEnumerator PlayDelayedVoiceover()
    {
        yield return new WaitForSeconds(3f);

        if (Voiceover2 != null)
        {
            Voiceover2.Play();
            Debug.Log("Playing secondary voiceover");
        }

        yield return new WaitForSeconds(5f);

        if (combat_song != null)
        {
            combat_song.Play();
            Debug.Log("Playing combat music");
        }
    }


}
