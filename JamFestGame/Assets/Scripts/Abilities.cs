using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;


public enum AbilityType { Dash, DoubleJump, Grapple, Glide, Teleport, Shrink, Hover, SuperSpeed, None }
public class Abilities : MonoBehaviour
{
    [Header("Componets")]
    private Movement movement;
    private Collision collision;
    private Rigidbody2D rb;
    private BetterJumping betterJumping;
    private AnimationScript anim;
    private GhostTrail ghostTrail;
    private SpriteRenderer sr;
    private BoxCollider2D playerBoxCollider;

    public List<AbilityType> abilities = new List<AbilityType>();

    #region Ability State
    [Header("Ability States")]
    [field: SerializeField] public bool IsHovering { get; private set; }
    [field: SerializeField] public bool IsGliding { get; private set; }
    [field: SerializeField] public bool IsGrappling { get; private set; }
    [field: SerializeField] public bool IsSuperSpeed { get; private set; }
    [field: SerializeField] public bool IsShrinking { get; private set; }
    [field: SerializeField] public bool IsTeleporting { get; private set; }
    [field: SerializeField] public bool CanUseAbilities { get; set; } = true;
    [field: SerializeField] public bool CanTeleport { get; set; } = true;
    private bool hasGrappled = false;
    #endregion

    private float defaultSpeed;
    private float originalGravity;

    private Vector2 grappleTarget;
    private Vector2 directionToGrappleTarget;


    private Vector2 originalCapsulesSize;
    private Vector2 originalCapsuleOffset;


    #region Ability Instances
    [Header("Ability Settings")]
    public TeleportSettings teleport = new TeleportSettings();
    public SuperSpeedSettings superSpeed = new SuperSpeedSettings();
    public GrappleSettings grapple = new GrappleSettings();
    public GlideSettings glide = new GlideSettings();
    public HoverSettings hover = new HoverSettings();
    #endregion

    public KeyCode teleportKey = KeyCode.T;
    public KeyCode grappleKey = KeyCode.H;
    public KeyCode superSpeedKey = KeyCode.J;
    public KeyCode glideKey = KeyCode.G;
    public KeyCode shrinkKey = KeyCode.F;
    public KeyCode dashkKey = KeyCode.P;
    public KeyCode doubleJumpKey = KeyCode.O;
    public KeyCode hoverKey = KeyCode.K;

    private Dictionary<AbilityType, string> abilityLetters = new Dictionary<AbilityType, string>()
    {
        { AbilityType.Dash, "" },
        { AbilityType.DoubleJump, "" },
        { AbilityType.Grapple, "" },
        { AbilityType.Glide, "" },
        { AbilityType.Teleport, "" },
        { AbilityType.Shrink, "" },
        { AbilityType.Hover, "" },
        { AbilityType.SuperSpeed, "" },
        { AbilityType.None, "" }
    };

    private string[] keyStrings = new string[] { "Y", "H", "U", "J", "I", "K", "O", "L", "P", "Semicolon", "LeftBracket", "RightBracket", "Quote" };


    void Awake()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
        anim = GetComponentInChildren<AnimationScript>();
        sr = GetComponent<SpriteRenderer>();
        ghostTrail = FindAnyObjectByType<GhostTrail>();
        playerBoxCollider = GetComponent<BoxCollider2D>();
    }

    void Start()
    {

        defaultSpeed = movement.speed;
        originalGravity = rb.gravityScale;

        if (playerBoxCollider != null)
        {
            originalCapsulesSize = playerBoxCollider.size;
            originalCapsuleOffset = playerBoxCollider.offset;
        }

        if (teleport.Curve == null)
            teleport.Curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        glide.Timer = glide.MaxTime;
    }
    void Update()
    {
        // --- GENERAL MOVEMENT ---
        if (!IsSuperSpeed)
            movement.speed = Mathf.Lerp(movement.speed, defaultSpeed, superSpeed.DeaccelerateSpeed);

        // Reset glide timer when grounded
        if (collision.onGround)
        {
            glide.Timer = glide.MaxTime;
            CanTeleport = true;
        }

        // --- ABILITY INPUTS ---
        if (Input.GetKeyDown(hoverKey) && CanUseAbilities)
        {
            StartCoroutine(HoverAbility());
        }
        if (Input.GetKeyDown(glideKey) && glide.Timer > 0 && CanUseAbilities && !IsGliding && HasAbility(AbilityType.Glide))
        {
            Glide();
        }
        if (Input.GetKeyDown(shrinkKey) && HasAbility(AbilityType.Shrink))
        {
            Shrink();
        }
        if (Input.GetKeyDown(teleportKey) && !IsTeleporting && !IsHovering && HasAbility(AbilityType.Teleport) && CanTeleport && CanUseAbilities)
        {
            StartCoroutine(TeleportSequence());
        }
        if (Input.GetKeyDown(grappleKey) && !IsHovering && HasAbility(AbilityType.Grapple) && CanUseAbilities)
        {
            GrappleHook();
        }
        if (Input.GetKey(superSpeedKey) && !IsHovering && (collision.onGround || IsSuperSpeed) && HasAbility(AbilityType.SuperSpeed) && CanUseAbilities)
        {
            SuperSpeed();

            if (!collision.onGround && superSpeed.SpeedParticle && superSpeed.SpeedParticle.isPlaying)
                superSpeed.SpeedParticle.Stop();

            else
            {
                IsSuperSpeed = false;
                if (superSpeed.SpeedParticle && superSpeed.SpeedParticle.isPlaying)
                    superSpeed.SpeedParticle.Stop();
            }
        }

        if (IsGliding && glide.Timer > 0) 
        { 
            UpdatePlayerStats(movement.canMove, CanUseAbilities, gravityScale: 0.5f, betterJumping: false); 
            glide.Timer -= Time.deltaTime; 
                if (glide.Timer <= 0) 
                IsGliding = false; 
        }

        if (!(IsGliding || IsGrappling) || collision.onGround)
        {
            IsGliding = false;
            UpdatePlayerStats(movement.canMove, CanUseAbilities, gravityScale: 3f, betterJumping: true);
        }            
    }
    void FixedUpdate()
    {
        if (IsGrappling)
        {
            Vector2 toTarget = grappleTarget - rb.position;
            float distance = toTarget.magnitude;
            Vector2 directionToTarget = toTarget.normalized;

            float moveStep = grapple.Speed * Time.fixedDeltaTime;

            if (distance > 0.1f)
            {
                rb.MovePosition(rb.position + directionToTarget * Mathf.Min(moveStep, distance));
                rb.gravityScale = 0f;
            }
            else 
            {
                IsGrappling = false;
                UpdatePlayerStats(canMove: true, CanUseAbilities, gravityScale: 3f, betterJumping: true);

                rb.linearVelocity = directionToGrappleTarget * grapple.Boost;

                anim.SetBool("isGrappling", false);
            }
        }

        // Maybe we move this to movement at somepoint?
        if (rb.linearVelocity.magnitude > movement.maxVelocity)
            rb.linearVelocity = rb.linearVelocity.normalized * movement.maxVelocity;
    }

    // ========== ABILITY METHODS ==========

    IEnumerator HoverAbility()
    {
        if (IsHovering || !CanUseAbilities || !HasAbility(AbilityType.Hover) || collision.onGround)
            yield break;

        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(0.2f, 0.5f, 14, 90, false, true);

        IsHovering = true;
        float originalGravity = rb.gravityScale;
        UpdatePlayerStats(canMove: false, canUseAbilities: false, gravityScale: 0, betterJumping.isActiveAndEnabled);


        RigidbodyConstraints2D originalConstraints = rb.constraints;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

        if (anim != null)
            anim.SetBool("isHovering", true);

        if (hover.HoverParticle != null)
            hover.HoverParticle.Play();

        float timer = 0f;
        while (timer < hover.Duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        UpdatePlayerStats(canMove: true, canUseAbilities: true, gravityScale: originalGravity, betterJumping.isActiveAndEnabled);
        rb.constraints = originalConstraints;

        if (anim != null)
            anim.SetBool("isHovering", false);

        if (hover.HoverParticle != null)
            hover.HoverParticle.Stop();
    }

    public void Glide()
    {
        IsGliding = true;
       // StartCoroutine(GlideCoroutine(glide.MaxTime));
    }




    public void GrappleHook()
    {
        CircleCollider2D grappleRange = GetComponentInChildren<CircleCollider2D>();
        Collider2D[] found = Physics2D.OverlapCircleAll(transform.position, grappleRange.radius);
        List<Collider2D> grapplePoints = new List<Collider2D>();

        if(found.Length == 0)
            return;

        foreach (var col in found)
            if (col.CompareTag("GrapplePoint"))
                grapplePoints.Add(col);


        Collider2D closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Collider2D targPoint in grapplePoints)
        {
            float distance = Vector2.Distance(transform.position, targPoint.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = targPoint;
            }
        }

        if (closest != null)
        {

            IsGrappling = true;
            UpdatePlayerStats(canMove: false, CanUseAbilities, gravityScale: 0, betterJumping: false);

            grappleTarget = closest.transform.position;
            directionToGrappleTarget = grappleTarget - rb.position;

            anim.Flip(grappleTarget.x < transform.position.x ? -1 : 1);
            anim.SetTrigger("grappleCast");
            anim.SetBool("isGrappling", true);

            SFXManager.Instance.Play(SFXManager.Instance.grappleClip, 1f, 0.95f, 1.05f);
            if (ghostTrail != null)
                ghostTrail.ShowGhost();
        }

        
    }

    public void Shrink()
    {
        IsShrinking = !IsShrinking;
        CanUseAbilities = IsShrinking ? false : true;
        AudioSource sfxToPlay = IsShrinking ? SFXManager.Instance.shrinkClip : SFXManager.Instance.unshrinkClip;
        SFXManager.Instance.Play(sfxToPlay, 1f);
        collision.ChangeSize(IsShrinking);
    }

    public void SuperSpeed()
    {
        IsSuperSpeed = true;
        movement.speed = defaultSpeed * 3;

        if (collision.onGround && superSpeed.SpeedParticle && !superSpeed.SpeedParticle.isPlaying)
            superSpeed?.SpeedParticle.Play();
    }

    public void SuperSpeedJump(float jumpForce)
    {
        if (collision.onGround)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    IEnumerator TeleportSequence()
    {
        
        CanTeleport = false;
        UpdatePlayerStats(canMove: false, canUseAbilities: CanUseAbilities, gravityScale: 0, betterJumping: false);
        IsTeleporting = true;
        

        yield return StartCoroutine(TeleportEffect(teleportStart: true, scale: transform.localScale));

        // Check for colliders at the intended teleport location
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;
        Vector2 intendedEnd = (Vector2)transform.position + dir * teleport.Force;
        float checkRadius = playerBoxCollider.bounds.extents.magnitude * 0.9f;
        Collider2D hitAtEnd = Physics2D.OverlapCircle(intendedEnd, checkRadius);

        if (hitAtEnd == null || hitAtEnd == collision)
        {
            transform.position = intendedEnd;
        }
        else
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, teleport.Force);
            Vector2 farthestHitPoint = Vector2.zero;
            float farthestDistance = -Mathf.Infinity;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != playerBoxCollider)
                {
                    float distance = Vector2.Distance(transform.position, hit.point);
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        farthestHitPoint = hit.point;
                    }
                }
            }

            if (farthestHitPoint != Vector2.zero)
                transform.position = farthestHitPoint + (-dir * (playerBoxCollider.bounds.extents.magnitude + 0.1f));
        }

        yield return StartCoroutine(TeleportEffect(teleportStart: false, scale: Vector3.one));

        UpdatePlayerStats(canMove: true, canUseAbilities: CanUseAbilities, gravityScale: originalGravity, betterJumping: true);
        IsTeleporting = false;
    }
    IEnumerator TeleportEffect(bool teleportStart, Vector3 scale)
    {
        float duration = teleport.Duration / 2f;
        float t = 0;

        if (teleportStart)
        {
            SFXManager.Instance.Play(SFXManager.Instance.teleportOutClip, 1f);
            SFXManager.Instance.Play(SFXManager.Instance.teleportInClip, 1f);

            if (ghostTrail != null)
                ghostTrail.ShowGhost();

            if (teleport.InEffect)
                teleport.InEffect.Play();
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;       // THESE 2
            float scaleFactor = teleportStart ? Mathf.Lerp(1f, 0f, teleport.Curve.Evaluate(p)) : Mathf.Lerp(0f, 1f, teleport.Curve.Evaluate(p));
            transform.localScale = scale * scaleFactor;       /// THESE
            if (sr) 
                sr.color = teleportStart ? new Color(1, 1, 1, Mathf.Lerp(1f, 0.2f, p)) : new Color(1, 1, 1, Mathf.Lerp(0.2f, 1f, p));
            yield return null;
        }

        if (teleportStart && teleport.InEffect)
            {
                ParticleSystem instance = Instantiate(teleport.InEffect, transform.position, Quaternion.identity);
                Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
            }
        else if (!teleportStart && teleport.OutEffect)
            teleport.OutEffect.Play();
    }
 

    public void AddAbility(AbilityType ability) => abilities.Add(ability);
    public bool HasAbility(AbilityType ability) => abilities.Contains(ability);

    public void UpdatePlayerStats(bool canMove, bool canUseAbilities, float gravityScale, bool betterJumping)
    {
        movement.canMove = canMove;
        CanUseAbilities = canUseAbilities;
        rb.gravityScale = gravityScale;
        this.betterJumping.enabled = betterJumping;
    }

    public void ResetAbilities()
    {
        // Create a temporary list of available keys
        List<string> availableKeys = new List<string>(keyStrings);

        foreach (AbilityType ability in abilities)
        {
            if (availableKeys.Count == 0)
                break; // No more keys to assign

            int index = Random.Range(0, availableKeys.Count);
            KeyCode randomKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), availableKeys[index]);

            switch (ability)
            {
                case AbilityType.Grapple:
                    grappleKey = randomKey;
                    break;
                case AbilityType.Teleport:
                    teleportKey = randomKey;
                    break;
                case AbilityType.SuperSpeed:
                    superSpeedKey = randomKey;
                    break;
                case AbilityType.Glide:
                    glideKey = randomKey;
                    break;
                case AbilityType.Shrink:
                    shrinkKey = randomKey;
                    break;
                case AbilityType.Dash:
                    dashkKey = randomKey;
                    break;
                case AbilityType.DoubleJump:
                    doubleJumpKey = randomKey;
                    break;
                case AbilityType.Hover:
                    hoverKey = randomKey;
                    break;
                    // Add more cases for other abilities with keys as needed
            }

            abilityLetters[ability] = randomKey.ToString();
            availableKeys.RemoveAt(index); // Remove used key
        }

        FindAnyObjectByType<HUDController>()?.UpdateKeyIcons(abilityLetters);
    }
}


#region Ability Settings Classes
[System.Serializable]
public class TeleportSettings
{
    public float Force = 5f;
    public float Duration = 0.3f;
    public AnimationCurve Curve;
    public ParticleSystem InEffect;
    public ParticleSystem OutEffect;
}

[System.Serializable]
public class SuperSpeedSettings
{
    public ParticleSystem SpeedParticle;
    public float DeaccelerateSpeed = 0.15f;
}

[System.Serializable]
public class GrappleSettings
{
    public float Speed = 10f;
    public float Boost = 50f;
}

[System.Serializable]
public class GlideSettings
{
    public float MaxTime = 2f;
    public float Timer;
}

[System.Serializable]
public class HoverSettings
{
    public float Duration = 2f;
    public ParticleSystem HoverParticle;
}
#endregion