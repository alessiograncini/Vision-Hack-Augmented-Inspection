using System;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using System.Collections;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class PointCloudVisualizer : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particleSystem;
    
    
    private ParticleSystem.Particle[] particles;

    [SerializeField] private string serverUrl = "http://192.168.4.38:5001/get_point_cloud";
    [SerializeField] private float updateInterval = 0.1f;
    [SerializeField] private int maxParticles = 100000;
    [SerializeField] private float particleSize = 0.01f;

    private Material particleMaterial;

    private void Start()
    {
        SetupParticleSystem();
        StartCoroutine(UpdatePointCloudRoutine());
    }

    [SerializeField]
    //private ParticleSystem _particleSystem;

    private void SetupParticleSystem()
    {
        //particleSystem = gameObject.AddComponent<ParticleSystem>();
        //particleSystem = _particleSystem;
        var main = particleSystem.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = maxParticles;
        main.simulationSpace = ParticleSystemSimulationSpace.Local;
        var emission = particleSystem.emission;
        emission.enabled = false;

        var shape = particleSystem.shape;
        shape.enabled = false;
        /*
      

        // Create and set up the particle material
        particleMaterial = new Material(Shader.Find("TextMeshPro/Mobile/Distance Field"));
        // particleMaterial = new Material(Shader.Find("Particles/Standard Unlit"));
        particleMaterial.SetColor("_EmissionColor", Color.white);

        var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = particleMaterial;
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        */

        particles = new ParticleSystem.Particle[maxParticles];
        
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

            int particleCount = Mathf.Min(points.Length, maxParticles);

            NativeArray<Vector3> positions = new NativeArray<Vector3>(particleCount, Allocator.TempJob);
            NativeArray<Color> particleColors = new NativeArray<Color>(particleCount, Allocator.TempJob);

            for (int i = 0; i < particleCount; i++)
            {
                positions[i] = new Vector3(points[i][0], points[i][1], points[i][2]);
                particleColors[i] = new Color(colors[i][0], colors[i][1], colors[i][2], 1f);
            }

            UpdateParticlesJob job = new UpdateParticlesJob
            {
                Positions = positions,
                Colors = particleColors,
                Particles = new NativeArray<ParticleSystem.Particle>(particles, Allocator.TempJob),
                ParticleSize = particleSize
            };

            JobHandle jobHandle = job.Schedule(particleCount, 64);
            jobHandle.Complete();

            job.Particles.CopyTo(particles);

            particleSystem.SetParticles(particles, particleCount);

            positions.Dispose();
            particleColors.Dispose();
            job.Particles.Dispose();
        }
    }

    [BurstCompile]
    private struct UpdateParticlesJob : IJobParallelFor
    {
        public NativeArray<Vector3> Positions;
        public NativeArray<Color> Colors;
        public NativeArray<ParticleSystem.Particle> Particles;
        public float ParticleSize;

        public void Execute(int index)
        {
            ParticleSystem.Particle particle = Particles[index];
            particle.position = Positions[index];
            particle.startColor = Colors[index];
            particle.startSize = ParticleSize;
            particle.remainingLifetime = float.PositiveInfinity;
            Particles[index] = particle;
        }
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
        if (particleMaterial != null)
        {
            Destroy(particleMaterial);
        }
    }
}
