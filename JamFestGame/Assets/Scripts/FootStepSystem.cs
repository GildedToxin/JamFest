using UnityEngine;

public enum SurfaceType { Default, Grass, Stone, Cloud }

[RequireComponent(typeof(Movement), typeof(Collision))]
public class FootstepSystem : MonoBehaviour
{
    [Header("Step Settings")]
    public float stepInterval = 0.35f;       // Time between steps if using movement-based triggering
    public float pitchMin = 0.95f;           // Pitch variation min
    public float pitchMax = 1.05f;           // Pitch variation max

    [Header("Surface AudioSources")]
    public AudioSource defaultStepSource;
    public AudioSource grassStepSource;
    public AudioSource stoneStepSource;
    public AudioSource cloudStepSource;

    private Movement movement;
    private Collision coll;

    private SurfaceType currentSurface = SurfaceType.Default;
    private float stepTimer = 0f;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float rayDistance = 0.2f;

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
    /// Use a raycast to detect the surface under the player
    /// </summary>
    void UpdateSurface()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, rayDistance, groundLayer);
        if (hit.collider != null)
        {
            switch (hit.collider.tag)
            {
                case "Grass": currentSurface = SurfaceType.Grass; break;
                case "Stone": currentSurface = SurfaceType.Stone; break;
                case "Cloud": currentSurface = SurfaceType.Cloud; break;
                default: currentSurface = SurfaceType.Default; break;
            }
        }
        else
        {
            currentSurface = SurfaceType.Default;
        }
    }

    /// <summary>
    /// Call this to play a footstep sound
    /// </summary>
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

    /// <summary>
    /// Optional: Call from animation events for perfect timing
    /// </summary>
    public void StepEvent()
    {
        PlayFootstep();
    }
}
