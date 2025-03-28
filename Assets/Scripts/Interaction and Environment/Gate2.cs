using UnityEngine;

public class Gate2 : MonoBehaviour
{
    public float openHeight = 5f; // How high the gate moves up
    public float openSpeed = 2f; // Speed of opening
    private GameManager gameManager;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private bool shouldOpen = false;
    public AudioSource Gate_open_audio;

    void Start()
    {
        gameManager = GameManager.Instance;
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.up * openHeight;
    }

    void Update()
    {
        // Check if all objects with the "Room1" tag are gone
        if (!shouldOpen && AreAllRoom1ObjectsCleared())
        {
            OpenGate();
        }

        // Move the gate if it should open
        if (shouldOpen)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, openSpeed * Time.deltaTime);
        }
    }

    private bool AreAllRoom1ObjectsCleared()
    {
        // Check if there are any GameObjects with the "Room1" tag
        GameObject[] room1Objects = GameObject.FindGameObjectsWithTag("Room1");
        return room1Objects.Length == 0;
    }

    public void OpenGate()
    {
        shouldOpen = true;
        Debug.Log("Gate is opening...");

        if (Gate_open_audio != null)
        {
            Gate_open_audio.Play();
        }
        gameManager.playerRoomNumber = 2;
        // Optionally add sound or animation trigger here
    }
}