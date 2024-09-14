using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class GridPointCloudVisualizer : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://localhost:5001/get_point_cloud";
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int gridSize = 100;
    [SerializeField] private float gridSpacing = 0.1f;
    [SerializeField] private float activationDistance = 0.2f;

    private GameObject[,,] grid;
    private Vector3 gridOffset;

    private void Start()
    {
        InitializeGrid();
        StartCoroutine(UpdatePointCloudRoutine());
    }

    private void InitializeGrid()
    {
        grid = new GameObject[gridSize, gridSize, gridSize];
        gridOffset = new Vector3(gridSize / 2f, gridSize / 2f, gridSize / 2f) * gridSpacing;

        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    Vector3 position = new Vector3(x, y, z) * gridSpacing - gridOffset;
                    GameObject cube = Instantiate(cubePrefab, position, Quaternion.identity, transform);
                    cube.transform.localScale = Vector3.one * gridSpacing;
                    cube.SetActive(false);
                    grid[x, y, z] = cube;
                }
            }
        }
    }

    private IEnumerator UpdatePointCloudRoutine()
    {
        while (true)
        {
            yield return StartCoroutine(FetchPointCloudData());
            yield return new WaitForSeconds(updateInterval);
        }
    }

    private IEnumerator FetchPointCloudData()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(serverUrl))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Error: {www.error}");
            }
            else
            {
                string jsonResult = www.downloadHandler.text;
                ProcessPointCloudData(jsonResult);
            }
        }
    }

    private void ProcessPointCloudData(string jsonData)
    {
        JObject data = JObject.Parse(jsonData);

        if (data["status"].ToString() == "success")
        {
            float[][] points = data["points"].ToObject<float[][]>();
            float[][] colors = data["colors"].ToObject<float[][]>();

            // Deactivate all cubes
            DeactivateAllCubes();

            // Activate and color cubes near points
            for (int i = 0; i < points.Length; i++)
            {
                Vector3 point = new Vector3(points[i][0], points[i][1], points[i][2]);
                Color color = new Color(colors[i][0], colors[i][1], colors[i][2]);
                ActivateNearbyGridCubes(point, color);
            }

            Debug.Log($"Updated point cloud with {points.Length} points");
        }
    }

    private void DeactivateAllCubes()
    {
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                for (int z = 0; z < gridSize; z++)
                {
                    grid[x, y, z].SetActive(false);
                }
            }
        }
    }

    private void ActivateNearbyGridCubes(Vector3 point, Color color)
    {
        Vector3 gridPoint = point + gridOffset;
        Vector3Int gridCoord = new Vector3Int(
            Mathf.RoundToInt(gridPoint.x / gridSpacing),
            Mathf.RoundToInt(gridPoint.y / gridSpacing),
            Mathf.RoundToInt(gridPoint.z / gridSpacing)
        );

        int searchRange = Mathf.CeilToInt(activationDistance / gridSpacing);

        for (int x = -searchRange; x <= searchRange; x++)
        {
            for (int y = -searchRange; y <= searchRange; y++)
            {
                for (int z = -searchRange; z <= searchRange; z++)
                {
                    Vector3Int checkCoord = gridCoord + new Vector3Int(x, y, z);

                    if (IsInGridBounds(checkCoord))
                    {
                        Vector3 cubePosition = GetCubeWorldPosition(checkCoord);
                        float distance = Vector3.Distance(point, cubePosition);

                        if (distance <= activationDistance)
                        {
                            GameObject cube = grid[checkCoord.x, checkCoord.y, checkCoord.z];
                            cube.SetActive(true);
                            cube.GetComponent<Renderer>().material.color = color;
                        }
                    }
                }
            }
        }
    }

    private bool IsInGridBounds(Vector3Int coord)
    {
        return coord.x >= 0 && coord.x < gridSize &&
               coord.y >= 0 && coord.y < gridSize &&
               coord.z >= 0 && coord.z < gridSize;
    }

    private Vector3 GetCubeWorldPosition(Vector3Int gridCoord)
    {
        return new Vector3(gridCoord.x, gridCoord.y, gridCoord.z) * gridSpacing - gridOffset;
    }
}
