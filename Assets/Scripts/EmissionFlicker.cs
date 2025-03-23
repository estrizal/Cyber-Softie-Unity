using UnityEngine;
using System.Collections;

public class EmissionFlicker : MonoBehaviour
{
    [Header("Flicker Settings")]
    public float minIntensity = 0f; // Minimum intensity of emission during flickering
    public float maxIntensity = 2f; // Maximum intensity of emission during flickering
    public float flickerSpeed = 0.1f; // Speed of flickering in seconds

    private Material[] emissiveMaterials; // Array to hold emissive materials
    private Color[] originalEmissionColors; // Array to store original emission colors
    private bool isFlickering = true;

    void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogError($"EmissionFlicker: No Renderer found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Filter out materials with emission enabled
        emissiveMaterials = renderer.materials;
        originalEmissionColors = new Color[emissiveMaterials.Length];

        for (int i = 0; i < emissiveMaterials.Length; i++)
        {
            if (emissiveMaterials[i].IsKeywordEnabled("_EMISSION"))
            {
                originalEmissionColors[i] = emissiveMaterials[i].GetColor("_EmissionColor"); // Store the exact original emission color
            }
            else
            {
                emissiveMaterials[i] = null; // Skip non-emissive materials
            }
        }

        StartCoroutine(FlickerRoutine());

        // Subscribe to electricity fix event
        FixElectricity.OnElectricityFixed += StopFlickering;
    }

    IEnumerator FlickerRoutine()
    {
        while (isFlickering)
        {
            for (int i = 0; i < emissiveMaterials.Length; i++)
            {
                if (emissiveMaterials[i] != null) // Only flicker emissive materials
                {
                    float intensity = Random.Range(minIntensity, maxIntensity);
                    emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i] * intensity);
                }
            }
            yield return new WaitForSeconds(flickerSpeed);
        }

        // Reset emission to its exact original color and intensity when flickering stops
        for (int i = 0; i < emissiveMaterials.Length; i++)
        {
            if (emissiveMaterials[i] != null)
            {
                emissiveMaterials[i].SetColor("_EmissionColor", originalEmissionColors[i]); // Restore exact original color and intensity
            }
        }
    }

    void StopFlickering()
    {
        isFlickering = false;
        Debug.Log($"Electricity fixed: Stopped flickering on {gameObject.name}");
    }

    void OnDestroy()
    {
        FixElectricity.OnElectricityFixed -= StopFlickering;
    }
}
