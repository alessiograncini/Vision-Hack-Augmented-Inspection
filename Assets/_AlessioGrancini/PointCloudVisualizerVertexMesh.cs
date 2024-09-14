using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class PointCloudVisualizerVertexMesh : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://192.168.1.225:5001/get_point_cloud";
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private int maxPoints = 1000000;
    [SerializeField] private float pointSize = 0.1f;

    private ComputeBuffer pointBuffer;
    private ComputeBuffer argsBuffer;
    private Material pointCloudMaterial;

    private struct PointData
    {
        public Vector3 position;
        public Vector3 color;
    }

    private Bounds pointCloudBounds;

    private void Start()
    {
        SetupPointCloud();
        StartCoroutine(UpdatePointCloudRoutine());
    }

    private void SetupPointCloud()
    {
        try
        {
            // Initialize compute buffer
            pointBuffer = new ComputeBuffer(maxPoints, sizeof(float) * 6, ComputeBufferType.Default);

            // Initialize args buffer
            uint[] args = new uint[5] { 0, 1, 0, 0, 0 };
            argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);

            // Load shader and create material
            Shader shader = Shader.Find("Custom/PointCloudComputeShader");
            if (shader == null)
            {
                Debug.LogError("Shader 'Custom/PointCloudComputeShader' not found.");
                return;
            }
            pointCloudMaterial = new Material(shader);
            pointCloudMaterial.SetBuffer("pointBuffer", pointBuffer);
            pointCloudMaterial.SetFloat("_PointSize", pointSize);

            // Initialize bounds
            pointCloudBounds = new Bounds(transform.position, Vector3.one * 10000f); // Adjust size as needed
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in SetupPointCloud: {ex.Message}");
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
                Debug.LogError($"Network Error: {www.error}");
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
        try
        {
            JObject data = JObject.Parse(jsonData);

            if (data["status"].ToString() == "success")
            {
                float[][] pointArray = data["points"].ToObject<float[][]>();
                float[][] colorArray = data["colors"].ToObject<float[][]>();

                int pointCount = Mathf.Min(pointArray.Length, maxPoints);
                Debug.Log($"Point Count: {pointCount}");

                // Prepare point data array
                PointData[] pointDataArray = new PointData[pointCount];
                for (int i = 0; i < pointCount; i++)
                {
                    pointDataArray[i] = new PointData
                    {
                        position = new Vector3(pointArray[i][0], pointArray[i][1], pointArray[i][2]),
                        color = new Vector3(colorArray[i][0], colorArray[i][1], colorArray[i][2])
                    };
                }

                // Update compute buffer
                pointBuffer.SetData(pointDataArray);

                // Update args buffer
                uint[] args = new uint[5] { (uint)pointCount, 1, 0, 0, 0 };
                argsBuffer.SetData(args);

                // Set material buffer
                pointCloudMaterial.SetBuffer("pointBuffer", pointBuffer);

                // Update bounds
                pointCloudBounds.center = transform.position;
            }
            else
            {
                Debug.LogError("Received unsuccessful status from server.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Exception in ProcessPointCloudData: {ex.Message}");
        }
    }

    private void Update()
    {
        if (pointCloudMaterial != null && pointBuffer != null && argsBuffer != null)
        {
            pointCloudMaterial.SetFloat("_PointSize", pointSize);
            pointCloudMaterial.SetMatrix("_LocalToWorld", transform.localToWorldMatrix);

            // Update bounds in case the GameObject has moved
            pointCloudBounds.center = transform.position;

            Debug.Log("Drawing Point Cloud");
            Graphics.DrawProceduralIndirect(pointCloudMaterial, pointCloudBounds, MeshTopology.Points, argsBuffer);
        }
        else
        {
            Debug.LogWarning("Cannot draw point cloud due to missing resources.");
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();

        if (pointBuffer != null)
        {
            pointBuffer.Release();
            pointBuffer = null;
        }
        if (argsBuffer != null)
        {
            argsBuffer.Release();
            argsBuffer = null;
        }
        if (pointCloudMaterial != null)
        {
            Destroy(pointCloudMaterial);
        }
    }
}
