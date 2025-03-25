using UnityEngine;

public class WindTunnel : MonoBehaviour
{
    [Header("Rotation Settings")]
    public Vector3 rotationAxis = Vector3.up; // Rotates along the Y-axis (default for fans)
    public float rotationSpeed = 360f; // Faster rotation speed (degrees per second)

    public AudioSource Fan_Sound;

    private bool shouldRotate = false;

    void OnEnable()
    {
        FixElectricity.OnElectricityFixed += StartRotating;
    }

    void OnDisable()
    {
        FixElectricity.OnElectricityFixed -= StartRotating;
    }

    void Update()
    {
        if (shouldRotate)
        {
            // Rotate continuously along the specified axis
            transform.Rotate(rotationAxis.normalized * rotationSpeed * Time.deltaTime);
        }
    }

    private void StartRotating()
    {
        shouldRotate = true;
        Debug.Log($"{gameObject.name} started rotating after electricity was fixed.");
        if ( Fan_Sound != null )
        {
            Fan_Sound.Play();
            Debug.Log("sound is playing");
        }

        // Optional: Add sound or animation trigger here
    }
}
