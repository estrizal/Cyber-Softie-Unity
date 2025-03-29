using UnityEngine;
using UnityEngine.SceneManagement;

public class Teleport_Player_Please : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // This function is called when the trigger collider is triggered
    private void OnTriggerEnter(Collider other)
    {

        SceneManager.LoadScene("Game-Re");

    }
}
