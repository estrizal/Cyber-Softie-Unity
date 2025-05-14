using UnityEngine;

public class SelfDuplicator : MonoBehaviour
{
    public float cloneInterval = 1f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= cloneInterval)
        {
            CloneSelf();
            timer = 0f;
        }
    }

    void CloneSelf()
    {
        // Clone the whole GameObject with children
        GameObject clone = Instantiate(gameObject, transform.position + GetRandomOffset(), transform.rotation);

        // Remove the SelfDuplicator from the clone to prevent exponential duplication
        Destroy(clone.GetComponent<SelfDuplicator>());

        // Get the Random_Movements component from the new clone
        Random_Movements cloneMovement = clone.GetComponent<Random_Movements>();
        if (cloneMovement != null)
        {
            // Find and reassign 'body' and 'groundCheck' from clone's children
            cloneMovement.body = FindChildByName(clone.transform, "Body");
            cloneMovement.groundCheck = FindChildByName(clone.transform, "GroundCheck");
        }
    }

    // Optionally spread the clones out a bit to avoid overlapping
    Vector3 GetRandomOffset()
    {
        return new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
    }

    // Finds child transform by name (recursive)
    Transform FindChildByName(Transform parent, string targetName)
    {
        foreach (Transform child in parent.GetComponentsInChildren<Transform>(true))
        {
            if (child.name == targetName)
                return child;
        }

        Debug.LogWarning($"Child named '{targetName}' not found in clone.");
        return null;
    }
}
