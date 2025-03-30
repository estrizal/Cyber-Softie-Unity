using UnityEngine;
using System.Collections; // Needed for coroutines

public class Final_Final_cutscene : MonoBehaviour
{
    public GameObject targetObject; // Input for the game object
    public GameObject cutscene1;
    public GameObject cutscene2;
    public GameObject knife;

    // This function is called when the trigger collider is triggered
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Change the target object's transform

            GhostCharacterController ghostController = targetObject.GetComponent<GhostCharacterController>();
            if (ghostController != null)
            {
                ghostController.enabled = false;
            }
            targetObject.transform.position = new Vector3(-0.824f, -0.29f, -13.98f);

            // Start the FixElectricitySequence coroutine
            StartCoroutine(FixElectricitySequence());
        }
    }

    private IEnumerator FixElectricitySequence()
    {
        // Step 1: Activate Cutscene 1
        cutscene1.SetActive(true);
        Debug.Log("Cutscene 1 activated.");

        yield return new WaitForSeconds(3f); // Wait for 3 seconds
        targetObject.transform.rotation = Quaternion.Euler(0f, -58.36f, 0f);
        knife.SetActive(true);

        // Step 2: Disable sparks and play electricity fixed sound

        // Step 3: Activate Cutscene 2 and deactivate Cutscene 1
        cutscene2.SetActive(true);
        cutscene1.SetActive(false);
        Debug.Log("Cutscene 2 activated and Cutscene 1 deactivated.");



        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        
        // Step 4: Activate WindPrefab and play electricity fixed sound again

        yield return new WaitForSeconds(1.5f); // Wait for 1.5 seconds

        //cutscene2.SetActive(false);
    }
}
