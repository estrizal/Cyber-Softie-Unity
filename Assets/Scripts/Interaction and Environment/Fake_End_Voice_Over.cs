using UnityEngine;

public class Fake_End_Voice_Over : MonoBehaviour
{
    public AudioSource voiceOver; // Input for the voice over audio source

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
        if (other.CompareTag("Player") && voiceOver != null)
        {
            voiceOver.Play();
        }
    }
}
