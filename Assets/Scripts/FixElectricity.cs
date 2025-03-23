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

    public GameObject cutscene1;
    public GameObject cutscene2;

    public void Interact()
    {
        if (isFixed) return;

        isFixed = true;

        Debug.Log("Electricity has been fixed!");

        // Trigger the event to notify all subscribed systems
        

        



        cutscene1.SetActive(true);

        [Add some wait time here for 1.5 seconds. ]

        if (sparksPrefab != null)
        {
            ElectricityFixed.Play();
            sparksPrefab.SetActive(false);
                OnElectricityFixed?.Invoke();

                [Add some wait time here for 1.5 seconds]


            
        }

        
        cutscene2.SetActive(true);
        cutscene1.SetActive(false);

        [Add wait time here for 1.5 seconds]

        if (WindPrefab != null)
        {
            WindPrefab.SetActive(true);
            ElectricityFixed.Play();

                [Add wait time here for 1.5 seconds]

            
        }

        // Optional: Add sound or animation feedback here
    }
}
