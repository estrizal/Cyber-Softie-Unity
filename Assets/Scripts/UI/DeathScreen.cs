using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DeathScreen : MonoBehaviour
{
    private Camera camera;
    public Button restartButton; // Reference to your restart button

    void Start()
    {
        camera = Camera.main;
        
        // Add listener to the restart button
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        else
        {
            Debug.LogError("Restart button not assigned to DeathScreen!");
        }
    }

    void Update()
    {
        camera.transform.Rotate(Vector3.up * Time.deltaTime * 10f);
    }

    private void RestartGame()
    {
        SceneManager.LoadScene("Game");
    }

}
