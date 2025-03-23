using UnityEngine;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour
{
    [Header("UI References")]
    public GameObject minigamePanel; // Reference to Minigame UI Panel
    public Text statusText; // Text to display status (e.g., "You Win!" or "Try Again")

    private bool isMinigameActive = false;

    public void StartMinigame()
    {
        if (isMinigameActive) return;

        isMinigameActive = true;

        // Show minigame panel
        minigamePanel.SetActive(true);

        // Disable normal gameplay controls
        Time.timeScale = 0f; // Pause game time
        Cursor.lockState = CursorLockMode.None; // Unlock cursor for UI interaction
        Cursor.visible = true;

        Debug.Log("Minigame started!");
    }

    public void CompleteMinigame()
    {
        isMinigameActive = false;

        // Hide minigame panel
        minigamePanel.SetActive(false);

        // Re-enable normal gameplay controls
        Time.timeScale = 1f; // Resume game time
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Minigame completed!");
    }

    public void QuitMinigame()
    {
        isMinigameActive = false;

        // Hide minigame panel
        minigamePanel.SetActive(false);

        // Re-enable normal gameplay controls
        Time.timeScale = 1f; // Resume game time
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Debug.Log("Minigame quit!");
    }
}
