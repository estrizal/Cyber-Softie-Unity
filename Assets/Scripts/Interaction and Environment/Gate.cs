using UnityEngine;

public class Gate : MonoBehaviour
{
    public float openHeight = 5f; // How high the gate moves up
    public float openSpeed = 2f; // Speed of opening

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool shouldOpen = false;
    public AudioSource Gate_open_audio;

    void Start()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.up * openHeight;
    }

    void Update()
    {
        if (shouldOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
        }
    }

    public void OpenGate()
    {
        shouldOpen = true;
        Debug.Log("Gate is opening...");

        if (Gate_open_audio != null)
        {
            Gate_open_audio.Play();
        }

        // Optionally add sound or animation trigger here
    }
}
