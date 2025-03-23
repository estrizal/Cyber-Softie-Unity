using UnityEngine;
using System;

public class FixElectricity : MonoBehaviour, IInteractable
{
    public static event Action OnElectricityFixed;

    [Header("Interaction Settings")]
    public string InteractionPrompt => "Press [E] to fix electricity";

    private bool isFixed = false;

    public AudioSource ElectricityFixed;

    public GameObject sparksPrefab;

    public GameObject WindPrefab;

    public void Interact()
    {
        if (isFixed) return;

        isFixed = true;

        Debug.Log("Electricity has been fixed!");

        // Trigger the event to notify all subscribed systems
        OnElectricityFixed?.Invoke();

        ElectricityFixed.Play();


        if (sparksPrefab != null)
        {
            sparksPrefab.SetActive(false);
            
        }

        if (WindPrefab != null)
        {
            WindPrefab.SetActive(true);
        }

        // Optional: Add sound or animation feedback here
    }
}
