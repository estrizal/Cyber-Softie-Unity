using UnityEngine;

public class AuthenticationButton : MonoBehaviour, IInteractable
{

    public string MESSAGE = "Press and hold 'E' to hack";
    public AudioSource authensucess;
    public string InteractionPrompt => MESSAGE;//"Fingerprint authentication board\nPress [E] to hack";

    public Gate gateToOpen; // Reference to the gate object

    private bool isActivated = false;

    public void Interact()
    {
        if (isActivated) return;

        isActivated = true;
        gateToOpen.OpenGate();
        Debug.Log("Authentication successful. Gate opening...");
        if (authensucess != null)
        {
            // Ensure background music loops
            authensucess.Play(); // Start playing background music
        }




        // Optionally add visual feedback here (e.g., change button color, play sound)
    }
}
