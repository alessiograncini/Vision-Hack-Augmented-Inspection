using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class BurstPointCloud : MonoBehaviour
{
    [SerializeField] private string serverUrl = "http://localhost:5001/get_point_cloud";
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private Mesh cubeMesh;
    [SerializeField] private Material cubeMaterial;
    [SerializeField] private int maxCubes = 100000;
    [SerializeField] private float cubeScale = 0.01f;

    private NativeArray<Vector3> positions;
    private NativeArray<Vector4> colors;
    private NativeArray<Matrix4x4> matrices;
    private MaterialPropertyBlock propertyBlock;
    private int activeCount;

    private void Start()
    {
        InitializeArrays();
        StartCoroutine(UpdatePointCloudRoutine());
    }

    private void InitializeArrays()
    {
        positions = new NativeArray<Vector3>(maxCubes, Allocator.Persistent);
        colors = new NativeArray<Vector4>(maxCubes, Allocator.Persistent);
        matrices = new NativeArray<Matrix4x4>(maxCubes, Allocator.Persistent);
        propertyBlock = new MaterialPropertyBlock();
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
            float[][] colorData = data["colors"].ToObject<float[][]>();

            activeCount = Mathf.Min(points.Length, maxCubes);

            var updateJob = new UpdatePointCloudJob
            {
                points = new NativeArray<Vector3>(points.Length, Allocator.TempJob),
                colors = new NativeArray<Vector4>(points.Length, Allocator.TempJob),
                outputPositions = positions,
                outputColors = colors,
                outputMatrices = matrices,
                scale = cubeScale
            };

            for (int i = 0; i < points.Length; i++)
            {
                updateJob.points[i] = new Vector3(points[i][0], points[i][1], points[i][2]);
                updateJob.colors[i] = new Vector4(colorData[i][0], colorData[i][1], colorData[i][2], 1f);
            }

            JobHandle jobHandle = updateJob.Schedule(activeCount, 64);
            jobHandle.Complete();

            updateJob.points.Dispose();
            updateJob.colors.Dispose();

            Debug.Log($"Updated point cloud with {activeCount} points");
        }
    }

    private void Update()
    {
        if (activeCount > 0)
        {
            propertyBlock.SetVectorArray("_BaseColor", colors.Slice(0, activeCount).ToArray());
            Graphics.DrawMeshInstanced(cubeMesh, 0, cubeMaterial, matrices.Slice(0, activeCount).ToArray(), activeCount, propertyBlock);
        }
    }

    private void OnDestroy()
    {
        if (positions.IsCreated) positions.Dispose();
        if (colors.IsCreated) colors.Dispose();
        if (matrices.IsCreated) matrices.Dispose();
    }
}

[BurstCompile]
public struct UpdatePointCloudJob : IJobParallelFor
{
    [ReadOnly] public NativeArray<Vector3> points;
    [ReadOnly] public NativeArray<Vector4> colors;
    [WriteOnly] public NativeArray<Vector3> outputPositions;
    [WriteOnly] public NativeArray<Vector4> outputColors;
    [WriteOnly] public NativeArray<Matrix4x4> outputMatrices;
    public float scale;

    public void Execute(int index)
    {
        outputPositions[index] = points[index];
        outputColors[index] = colors[index];
        outputMatrices[index] = Matrix4x4.TRS(points[index], Quaternion.identity, Vector3.one * scale);
    }
}
