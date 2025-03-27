using UnityEngine;
using System.Collections.Generic;

public class BulletTrail : MonoBehaviour
{
    public int trailLength = 20;
    public float trailWidth = 0.1f;
    public float minVertexDistance = 0.1f;
    public Material trailMaterial;

    private LineRenderer lineRenderer;
    private Queue<Vector3> trailPositions = new Queue<Vector3>();

    void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = trailMaterial;
        lineRenderer.startWidth = trailWidth;
        lineRenderer.endWidth = trailWidth;
        lineRenderer.positionCount = 0;
    }

    void Update()
    {
        if (trailPositions.Count == 0 || Vector3.Distance(transform.position, trailPositions.Peek()) > minVertexDistance)
        {
            trailPositions.Enqueue(transform.position);

            if (trailPositions.Count > trailLength)
            {
                trailPositions.Dequeue();
            }

            lineRenderer.positionCount = trailPositions.Count;
            lineRenderer.SetPositions(trailPositions.ToArray());
        }
    }

    public void ResetTrail()
    {
        trailPositions.Clear();
        lineRenderer.positionCount = 0;
    }
}