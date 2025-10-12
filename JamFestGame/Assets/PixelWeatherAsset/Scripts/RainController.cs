using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RainController : MonoBehaviour
{
    [Range(0, 1f)] public float masterIntensity = 1f;
    [Range(0, 1f)] public float rainIntensity = 0f;
    [Range(0, 1f)] public float windIntensity = 0f;
    [Range(0, 1f)] public float fogIntensity = 0f;
    [Range(0, 1f)] public float lightningIntensity = 0f;

    public bool autoUpdate = true;
    public bool dynamicWeather = true;

    [Header("Particle Systems")]
    public ParticleSystem rainPart;
    public ParticleSystem windPart;
    public ParticleSystem lightningPart;
    public ParticleSystem fogPart;

    [Header("Audio Sources")]
    public AudioSource rainAudio;  // assign your rain audio source
    public AudioSource windAudio;  // assign your wind audio source

    [Header("Audio Settings")]
    [Range(0, 1f)] public float rainMaxVolume = 0.7f;
    [Range(0, 1f)] public float windMaxVolume = 0.6f;

    private ParticleSystem.EmissionModule rainEmission;
    private ParticleSystem.ForceOverLifetimeModule rainForce;
    private ParticleSystem.EmissionModule windEmission;
    private ParticleSystem.MainModule windMain;
    private ParticleSystem.EmissionModule lightningEmission;
    private ParticleSystem.MainModule lightningMain;
    private ParticleSystem.EmissionModule fogEmission;

    private float rainSeed, windSeed, fogSeed, lightningSeed;

    void Awake()
    {
        rainEmission = rainPart.emission;
        rainForce = rainPart.forceOverLifetime;
        windEmission = windPart.emission;
        windMain = windPart.main;
        lightningEmission = lightningPart.emission;
        lightningMain = lightningPart.main;
        fogEmission = fogPart.emission;

        rainSeed = Random.Range(0f, 100f);
        windSeed = Random.Range(0f, 100f);
        fogSeed = Random.Range(0f, 100f);
        lightningSeed = Random.Range(0f, 100f);

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

        rainIntensity = SkewedPerlinWithZero(rainSeed, time, 4f, 0.1f);
        windIntensity = SkewedPerlin(windSeed, time, 0.3f);
        fogIntensity = SkewedPerlin(fogSeed, time, 0.3f);
        lightningIntensity = SkewedPerlin(lightningSeed, time, 0.15f);
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
        // --- Particle systems ---
        rainEmission.rateOverTime = 200f * masterIntensity * rainIntensity;
        rainForce.x = new ParticleSystem.MinMaxCurve(-25f * windIntensity * masterIntensity, (-3 - 30f * windIntensity) * masterIntensity);

        windEmission.rateOverTime = 5f * masterIntensity * (windIntensity + fogIntensity);
        windMain.startLifetime = 2f + 5f * (1f - windIntensity);
        windMain.startSpeed = new ParticleSystem.MinMaxCurve(15f * windIntensity, 25f * windIntensity);

        fogEmission.rateOverTime = (1f + (rainIntensity + windIntensity) * 0.5f) * fogIntensity * masterIntensity;

        lightningEmission.rateOverTime = (rainIntensity * masterIntensity < 0.7f) ? 0 : lightningIntensity * masterIntensity * 0.4f;

        // --- Audio ---
        if (rainAudio != null)
            rainAudio.volume = Mathf.Lerp(0f, rainMaxVolume, rainIntensity * masterIntensity);

        if (windAudio != null)
            windAudio.volume = Mathf.Lerp(0f, windMaxVolume, windIntensity * masterIntensity);
    }

    public void OnMasterChanged(float value) { masterIntensity = value; UpdateAll(); }
    public void OnRainChanged(float value) { rainIntensity = value; UpdateAll(); }
    public void OnWindChanged(float value) { windIntensity = value; UpdateAll(); }
    public void OnLightningChanged(float value) { lightningIntensity = value; UpdateAll(); }
    public void OnFogChanged(float value) { fogIntensity = value; UpdateAll(); }
}
