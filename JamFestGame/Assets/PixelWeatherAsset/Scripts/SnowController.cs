using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowController : MonoBehaviour
{
    [Range(0, 1f)] public float masterIntensity = 1f;
    [Range(0, 1f)] public float snowIntensity = 0f;
    [Range(0, 1f)] public float windIntensity = 0f;
    [Range(0, 1f)] public float fogIntensity = 0f;
    [Range(0, 7f)] public float snowLevel;

    public bool autoUpdate = true;
    public bool dynamicWeather = true; 

    public ParticleSystem snowPart;
    public ParticleSystem windPart;
    public ParticleSystem fogPart;

    private ParticleSystem.EmissionModule snowEmission;
    private ParticleSystem.ForceOverLifetimeModule snowForce;
    private ParticleSystem.ShapeModule snowShape;
    private ParticleSystem.EmissionModule windEmission;
    private ParticleSystem.MainModule windMain;
    private ParticleSystem.EmissionModule fogEmission;
    private Transform snowTransform;

    public Material snowMat;


    private float snowSeed, windSeed, fogSeed;

    void Awake()
    {
        snowTransform = snowPart.transform;
        snowEmission = snowPart.emission;
        snowShape = snowPart.shape;
        snowForce = snowPart.forceOverLifetime;
        windEmission = windPart.emission;
        windMain = windPart.main;
        fogEmission = fogPart.emission;

        snowSeed = Random.Range(0f, 100f);
        windSeed = Random.Range(0f, 100f);
        fogSeed = Random.Range(0f, 100f);

        UpdateAll();
    }

    void Update()
    {
        if (dynamicWeather)
            RandomizeWeather();

        if (autoUpdate)
            UpdateAll();
    }

    void RandomizeWeather()
    {
        float time = Time.time * 0.05f;

        snowIntensity = SkewedPerlinWithZero(snowSeed, time, 2f, 0.02f);

        windIntensity = SkewedPerlin(windSeed, time, 0.3f);
        fogIntensity = SkewedPerlin(fogSeed, time, 0.3f);
    }

    float SkewedPerlinWithZero(float seed, float time, float power, float zeroThreshold)
    {
        float n = Mathf.PerlinNoise(seed, time);
        n = Mathf.Pow(n, power); 
        if (n < zeroThreshold) n = 0f; 
        return Mathf.Clamp01(n);
    }


    float SkewedPerlin(float seed, float time, float bias)
    {
        float n = Mathf.PerlinNoise(seed, time);
        float power = Mathf.Lerp(1f, 3f, bias);
        n = Mathf.Pow(n, power);
        return Mathf.Clamp01(n);
    }

    void UpdateAll()
    {
   
        snowEmission.rateOverTime = 110f * masterIntensity * snowIntensity;
        snowShape.radius = 30f * Mathf.Clamp(windIntensity, 0.4f, 1f) * masterIntensity;

        float minX = windIntensity == 0 ? -0.5f : -3f - 14f * windIntensity;
        float maxX = windIntensity == 0 ? 0.5f : -9f * windIntensity;
        snowForce.x = new ParticleSystem.MinMaxCurve(minX, maxX);


        windEmission.rateOverTime = 14f * masterIntensity * (windIntensity + fogIntensity);
        windMain.startLifetime = 2f + 6f * (1f - windIntensity);
        windMain.startSpeed = new ParticleSystem.MinMaxCurve(15f * windIntensity, 20f * windIntensity);

        fogEmission.rateOverTime = (fogIntensity + (snowIntensity + windIntensity) * 0.5f) * masterIntensity;

    
        snowMat.SetFloat("_SnowLevel", snowLevel);
    }


    public void OnMasterChanged(float value) { masterIntensity = value; UpdateAll(); }
    public void OnSnowChanged(float value) { snowIntensity = value; UpdateAll(); }
    public void OnWindChanged(float value) { windIntensity = value; UpdateAll(); }
    public void OnFogChanged(float value) { fogIntensity = value; UpdateAll(); }
    public void OnSnowLevelChanged(float value) { snowLevel = value; UpdateAll(); }
}
