using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class CubePointCloudVisualizer : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://localhost:5001/get_point_cloud";
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private GameObject cubePrefab;
    [SerializeField] private int maxCubes = 100000;
    [SerializeField] private float cubeScale = 0.01f;

    private List<GameObject> cubePool;
    private int activeCubes = 0;

    private void Start()
    {
        InitializeCubePool();
        StartCoroutine(UpdatePointCloudRoutine());
    }

    private void InitializeCubePool()
    {
        cubePool = new List<GameObject>(maxCubes);
        for (int i = 0; i < maxCubes; i++)
        {
            GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, transform);
            cube.transform.localScale = Vector3.one * cubeScale;
            cube.SetActive(false);
            cubePool.Add(cube);
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

            int pointCount = Mathf.Min(points.Length, maxCubes);

            for (int i = 0; i < pointCount; i++)
            {
                GameObject cube = cubePool[i];
                cube.SetActive(true);
                cube.transform.localPosition = new Vector3(points[i][0], points[i][1], points[i][2]);
                cube.GetComponent<Renderer>().material.color = new Color(colors[i][0], colors[i][1], colors[i][2]);
            }

            // Deactivate unused cubes
            for (int i = pointCount; i < activeCubes; i++)
            {
                cubePool[i].SetActive(false);
            }

            activeCubes = pointCount;
            Debug.Log($"Updated point cloud with {pointCount} points");
        }
    }
}
