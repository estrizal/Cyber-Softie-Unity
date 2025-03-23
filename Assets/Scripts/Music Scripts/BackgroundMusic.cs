using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource != null)
        {
            audioSource.loop = true; // Ensure background music loops
            audioSource.Play(); // Start playing background music
        }
    }
}
