using UnityEngine;
using System.Collections; // Add this for coroutines

public class AuthenticationButton : MonoBehaviour, IInteractable
{
    public string MESSAGE = "Press and hold 'E' to hack";
    public AudioSource authensucess;
    public AudioSource Voiceover2;
    public AudioSource Bg_song;
    public string InteractionPrompt => MESSAGE;
    public Gate gateToOpen;

    private bool isActivated = false;

    public void Interact()
    {
        if (isActivated) return;

        isActivated = true;
        gateToOpen.OpenGate();
        Debug.Log("Authentication successful. Gate opening...");

        if (authensucess != null)
        {
            authensucess.Play();
            // Start coroutine to play voiceover after 3 seconds
            StartCoroutine(PlayDelayedVoiceover());
        }

        // Optional: Add visual feedback here
    }

    private IEnumerator PlayDelayedVoiceover()
    {
        yield return new WaitForSeconds(4f);

        if (Voiceover2 != null)
        {
            Voiceover2.Play();
            Debug.Log("Playing secondary voiceover");
        }
        
        yield return new WaitForSeconds(2f);

        if (Bg_song != null)
        {
            Bg_song.Play();
            Debug.Log("Playing background music");
        }

    }
}
