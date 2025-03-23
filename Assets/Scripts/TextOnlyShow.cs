using UnityEngine;
using UnityEngine.Audio;

public class TextMessageInteractable : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    [TextArea]
    [SerializeField] public string interactionMessage = "This is a simple text message."; // Message to display
    public AudioSource text_interaction_sound;
    public string InteractionPrompt => interactionMessage; // Display the message as the prompt


    public void Interact()
    {
        // No action is performed on interaction, just display the message
        Debug.Log($"Interacted with: {gameObject.name}");
        if (text_interaction_sound != null)
        {
            // Ensure background music loops
            text_interaction_sound.Play(); // Start playing background music
        }
    }
}
