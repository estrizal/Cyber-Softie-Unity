using UnityEngine;
using TMPro;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance;
    private TextMeshProUGUI promptText;
    private CanvasGroup canvasGroup;
    public AudioSource text_0nly_interaction;

    void Awake()
    {
        if (Instance == null) Instance = this;
        promptText = GetComponent<TextMeshProUGUI>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0; // Initially hidden
    }

    public void ShowPrompt(string prompt)
    {
        if (promptText.text != prompt)
            promptText.text = prompt;

        canvasGroup.alpha = 1; // Just change visibility smoothly
        text_0nly_interaction.Play();
    }

    public void HidePrompt()
    {
        canvasGroup.alpha = 0;
    }
}
