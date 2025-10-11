using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RippleEffect : MonoBehaviour
{
    [Header("Waveform Settings")]
    public AnimationCurve waveform = new AnimationCurve(
        new Keyframe(0.00f, 0.50f, 0, 0),
        new Keyframe(0.05f, 1.00f, 0, 0),
        new Keyframe(0.15f, 0.10f, 0, 0),
        new Keyframe(0.25f, 0.80f, 0, 0),
        new Keyframe(0.35f, 0.30f, 0, 0),
        new Keyframe(0.45f, 0.60f, 0, 0),
        new Keyframe(0.55f, 0.40f, 0, 0),
        new Keyframe(0.65f, 0.55f, 0, 0),
        new Keyframe(0.75f, 0.46f, 0, 0),
        new Keyframe(0.85f, 0.52f, 0, 0),
        new Keyframe(0.99f, 0.50f, 0, 0)
    );

    [Header("Visual Settings")]
    [Range(0.01f, 1.0f)] public float refractionStrength = 0.5f;
    public Color reflectionColor = Color.gray;
    [Range(0.01f, 1.0f)] public float reflectionStrength = 0.7f;
    [Range(1.0f, 3.0f)] public float waveSpeed = 1.25f;

    [Header("Emission Settings")]
    [Range(0.0f, 2.0f)] public float dropInterval = 0.5f;

    public Shader shader;


    class Droplet
    {
        public Vector2 position;
        public float time = 1000f;

        public void Reset(Vector2 pos)
        {
            position = pos;
            time = 0;
        }

        public void ResetRandom()
        {
            position = new Vector2(Random.value, Random.value);
            time = 0;
        }

        public void Update()
        {
            time += Time.deltaTime;
        }

        public Vector4 MakeShaderParameter(float aspect)
        {
            return new Vector4(position.x * aspect, position.y, time, 0);
        }
    }

    Droplet[] droplets;
    Texture2D gradTexture;
    Material material;
    float timer;
    int dropCount;

    void OnEnable()
    {
        if (shader == null)
            shader = Shader.Find("Hidden/RippleEffect");

        droplets = new Droplet[3];
        for (int i = 0; i < droplets.Length; i++)
            droplets[i] = new Droplet();

        gradTexture = new Texture2D(2048, 1, TextureFormat.Alpha8, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int i = 0; i < gradTexture.width; i++)
        {
            float x = (float)i / gradTexture.width;
            float a = waveform.Evaluate(x);
            gradTexture.SetPixel(i, 0, new Color(a, a, a, a));
        }
        gradTexture.Apply();

        material = new Material(shader)
        {
            hideFlags = HideFlags.DontSave
        };
        material.SetTexture("_GradTex", gradTexture);

        UpdateShaderParameters();

        Debug.Log("RippleEffect Awake: Shader and material initialized.");
    }

    void Update()
    {
        if (droplets == null)
            return;

        if (dropInterval > 0)
        {
            timer += Time.deltaTime;
            while (timer > dropInterval)
            {
                Emit();
                Debug.Log("RippleEffect: Random droplet emitted.");
                timer -= dropInterval;
            }
        }

        foreach (var d in droplets)
            d.Update();

        UpdateShaderParameters();
    }

    void UpdateShaderParameters()
    {
        var cam = GetComponent<Camera>();

        material.SetVector("_Drop1", droplets[0].MakeShaderParameter(cam.aspect));
        material.SetVector("_Drop2", droplets[1].MakeShaderParameter(cam.aspect));
        material.SetVector("_Drop3", droplets[2].MakeShaderParameter(cam.aspect));

        material.SetColor("_Reflection", reflectionColor);
        material.SetVector("_Params1", new Vector4(cam.aspect, 1, 1 / waveSpeed, 0));
        material.SetVector("_Params2", new Vector4(1, 1 / cam.aspect, refractionStrength, reflectionStrength));
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Debug.Log("RippleEffect: OnRenderImage called!");
        Graphics.Blit(source, destination, material);
    }


    public void Emit()
    {
        droplets[dropCount++ % droplets.Length]?.ResetRandom();
        Debug.Log("RippleEffect: Emit() called (random).");
    }

    public void Emit(Vector3 viewportPos)
    {
        droplets[dropCount++ % droplets.Length].Reset(new Vector2(viewportPos.x, viewportPos.y));
        Debug.Log("RippleEffect: Emit(Vector3) called at " + viewportPos);
    }
}
