using UnityEngine;

public class GridElementChecker : MonoBehaviour
{
    public Transform parentObject; // Reference to the parent object
    public float distanceThreshold = 5f; // Distance threshold for checking proximity
    private MeshRenderer meshRenderer; // MeshRenderer of the element

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        bool isNear = false;
        Material closestMaterial = null;
        float closestDistance = distanceThreshold;

        foreach (Transform child in parentObject)
        {
            float distance = Vector3.Distance(transform.position, child.position);
            if (distance < closestDistance)
            {
                isNear = true;
                closestDistance = distance;

                // Try to copy material from child if it has a MeshRenderer
                MeshRenderer childRenderer = child.GetComponent<MeshRenderer>();
                if (childRenderer != null)
                {
                    closestMaterial = childRenderer.material;
                }
            }
        }

        if (isNear)
        {
            meshRenderer.enabled = true;
            if (closestMaterial != null)
            {
                meshRenderer.material = closestMaterial;
            }
        }
        else
        {
            meshRenderer.enabled = false;
        }
    }
}