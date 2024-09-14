using UnityEngine;

public class GridElementCreator : MonoBehaviour
{
    public GameObject elementPrefab; // Prefab to instantiate in the grid
    public int gridSize = 10; // X by X grid size
    public float elementSize = 1f; // Size of each element

    public Transform parentObject; // Reference to the parent object whose children will be checked
    public float distanceThreshold = 5f; // Distance threshold for enabling/disabling MeshRenderer
    public float spacing = 5f;

    void Start()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 position = new Vector3(x * elementSize + spacing, y * elementSize + spacing,
                        z * elementSize + spacing);
                    GameObject element = Instantiate(elementPrefab, position, Quaternion.identity, transform);
                    element.transform.localScale = new Vector3(elementSize, elementSize, elementSize);

                    // Attach the GridElementChecker script to the element and initialize
                    GridElementChecker checker = element.AddComponent<GridElementChecker>();
                    checker.parentObject = parentObject;
                    checker.distanceThreshold = distanceThreshold;
                }
            }
        }
    }
}