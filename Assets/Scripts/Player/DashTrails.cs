using System.Collections;
using UnityEngine;

public class DashTrails : MonoBehaviour
{
    private InputReader inputReader;
    public float dashDuration = 0.2f;
    public float meshRefreshRate = 0.1f; // Time between mesh refreshes in seconds
    private bool isTrailActive;
    public Transform positionToSpawn;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    public Material trailMaterial; // Add this material reference
    public float destroyDelay = 1f; // How long trails last

    void Start()
    {
        inputReader = GetComponent<InputReader>();
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
    }

    void OnEnable()
    {
        inputReader.OnDashPerformed += HandleDashPerformed;
    }

    void OnDisable()
    {
        inputReader.OnDashPerformed -= HandleDashPerformed;
    }

    void HandleDashPerformed()
    {
        if (!isTrailActive)
        {
            isTrailActive = true;
            StartCoroutine(ActivateDashTrail());
        }
    }

    IEnumerator ActivateDashTrail()
    {
        float elapsedTime = 0f;

        while (elapsedTime < dashDuration)
        {
            // Create trail meshes
            foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
            {
                GameObject trailObject = new GameObject("Trail");
                trailObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                
                // Add components
                MeshRenderer meshRenderer = trailObject.AddComponent<MeshRenderer>();
                MeshFilter meshFilter = trailObject.AddComponent<MeshFilter>();
                
                // Create and bake mesh
                Mesh mesh = new Mesh();
                smr.BakeMesh(mesh);
                meshFilter.mesh = mesh;
                
                // Set material
                meshRenderer.material = trailMaterial;
                
                // Destroy after delay
                Destroy(trailObject, destroyDelay);
            }

            elapsedTime += meshRefreshRate;
            yield return new WaitForSeconds(meshRefreshRate);
        }

        isTrailActive = false;
    }
}
