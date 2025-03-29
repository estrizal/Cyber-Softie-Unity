using System.Collections;
using UnityEngine;

public class DashTrails : MonoBehaviour
{
    [Header("Trail Settings")]
    public float dashDuration = 0.2f;
    public float meshRefreshRate = 0.02f; // Increased refresh rate for smoother trails
    public Material trailMaterial;
    public float destroyDelay = 0.5f;
    public float fadeSpeed = 2f;

    private InputReader inputReader;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private bool isTrailActive;

    void Awake()
    {
        // Get components in Awake instead of Start
        inputReader = GetComponentInParent<InputReader>();
        skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

        if (inputReader == null)
        {
            Debug.LogError("No InputReader found on parent object!", this);
        }

        if (skinnedMeshRenderers.Length == 0)
        {
            Debug.LogError("No SkinnedMeshRenderers found in children!", this);
        }
    }

    void OnEnable()
    {
        if (inputReader != null)
        {
            inputReader.OnDashPerformed += HandleDashPerformed;
        }
    }

    void OnDisable()
    {
        if (inputReader != null)
        {
            inputReader.OnDashPerformed -= HandleDashPerformed;
        }
    }

    void HandleDashPerformed()
    {
        StartCoroutine(ActivateDashTrail());
    }

    IEnumerator ActivateDashTrail()
    {
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            CreateTrailObjects();
            yield return new WaitForSeconds(meshRefreshRate);
        }
    }

    void CreateTrailObjects()
    {
        foreach (SkinnedMeshRenderer smr in skinnedMeshRenderers)
        {
            // Create trail object
            GameObject trailObject = new GameObject($"Trail_{smr.name}");
            trailObject.transform.SetPositionAndRotation(smr.transform.position, smr.transform.rotation);
            
            // Add mesh components
            MeshFilter meshFilter = trailObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = trailObject.AddComponent<MeshRenderer>();
            
            // Bake and assign mesh
            Mesh mesh = new Mesh();
            smr.BakeMesh(mesh);
            meshFilter.mesh = mesh;
            
            // Setup material
            Material trailMaterialInstance = new Material(trailMaterial);
            meshRenderer.material = trailMaterialInstance;

            // Add fade out component
            StartCoroutine(FadeOutTrail(trailObject, trailMaterialInstance));
        }
    }

    IEnumerator FadeOutTrail(GameObject trailObject, Material material)
    {
        Color color = material.color;
        float alpha = color.a;

        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            material.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        Destroy(trailObject);
    }
}
