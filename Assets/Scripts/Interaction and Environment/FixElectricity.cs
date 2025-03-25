using UnityEngine;
using System;
using System.Collections; // Needed for coroutines

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

        // Start the sequence with wait times
        StartCoroutine(FixElectricitySequence());
    }

    private IEnumerator FixElectricitySequence()
    {
        // Step 1: Activate Cutscene 1
        cutscene1.SetActive(true);
        Debug.Log("Cutscene 1 activated.");

        yield return new WaitForSeconds(3f); // Wait for 1.5 seconds

        // Step 2: Disable sparks and play electricity fixed sound
        if (sparksPrefab != null)
        {
            ElectricityFixed.Play();
            sparksPrefab.SetActive(false);
            Debug.Log("Sparks disabled and electricity fixed sound played.");

            OnElectricityFixed?.Invoke();

            yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds
        }

        // Step 3: Activate Cutscene 2 and deactivate Cutscene 1
        cutscene2.SetActive(true);
        cutscene1.SetActive(false);
        Debug.Log("Cutscene 2 activated and Cutscene 1 deactivated.");

        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        // Step 4: Activate WindPrefab and play electricity fixed sound again
        if (WindPrefab != null)
        {
            WindPrefab.SetActive(true);
            ElectricityFixed.Play();
            Debug.Log("Wind activated and electricity fixed sound played again.");

            yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

            cutscene2.SetActive(false);
        }

        Debug.Log("Electricity fixing sequence completed.");
    }
}
