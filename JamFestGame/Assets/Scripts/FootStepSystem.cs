using UnityEngine;
using UnityEngine.Tilemaps;

public enum SurfaceType { Default, Grass, Stone, Cloud }

[RequireComponent(typeof(Movement), typeof(Collision))]
public class FootstepSystem : MonoBehaviour
{
    [Header("Step Settings")]
    public float stepInterval = 0.35f;
    public float pitchMin = 0.95f;
    public float pitchMax = 1.05f;

    [Header("Surface AudioSources")]
    public AudioSource defaultStepSource;
    public AudioSource grassStepSource;
    public AudioSource stoneStepSource;
    public AudioSource cloudStepSource;

    private Movement movement;
    private Collision coll;

    private SurfaceType currentSurface = SurfaceType.Default;
    private float stepTimer = 0f;

    [Header("Tilemap Settings")]
    public Tilemap groundTilemap;

    void Awake()
    {
        movement = GetComponent<Movement>();
        coll = GetComponent<Collision>();
    }

    void Update()
    {
        UpdateSurface();

        // Movement-based footstep triggering
        if (movement.rb.linearVelocity.magnitude > 0.1f && coll.onGround)
        {
            stepTimer += Time.deltaTime;
            if (stepTimer >= stepInterval)
            {
                PlayFootstep();
                stepTimer = 0f;
            }
        }
        else
        {
            stepTimer = stepInterval; // Reset timer when not moving
        }
    }

    /// <summary>
    /// Detect the surface the player is standing on via Tilemap
    /// </summary>
    void UpdateSurface()
    {
        if (groundTilemap == null)
        {
            currentSurface = SurfaceType.Default;
            return;
        }

        Vector3 worldPos = transform.position + Vector3.down * 0.1f; // Slightly below player feet
        Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);
        TileBase tile = groundTilemap.GetTile(cellPos);

        if (tile == null)
        {
            currentSurface = SurfaceType.Default;
            return;
        }

        string tileName = tile.name.ToLower();

        if (tileName.Contains("grass"))
            currentSurface = SurfaceType.Grass;
        else if (tileName.Contains("stone"))
            currentSurface = SurfaceType.Stone;
        else if (tileName.Contains("cloud"))
            currentSurface = SurfaceType.Cloud;
        else
            currentSurface = SurfaceType.Default;
    }

    public void PlayFootstep()
    {
        AudioSource source = defaultStepSource;

        switch (currentSurface)
        {
            case SurfaceType.Grass: source = grassStepSource; break;
            case SurfaceType.Stone: source = stoneStepSource; break;
            case SurfaceType.Cloud: source = cloudStepSource; break;
        }

        if (source != null)
            SFXManager.Instance.Play(source, 1f, pitchMin, pitchMax);
    }

    public void StepEvent()
    {
        PlayFootstep();
    }
}
