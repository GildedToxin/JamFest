using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


public enum AbilityType { Dash, DoubleJump, Grapple, Glide, Teleport, Shrink, Hover, SuperSpeed, None }
public class Abilities : MonoBehaviour
{
    private Movement movement;
    private Collision collision;
    private Rigidbody2D rb;
    private BetterJumping betterJumping;
    private AnimationScript anim;
    private GhostTrail ghostTrail;
    private SpriteRenderer sr;

    public bool isGrappling = false;
    public bool isTeleporting = false;
    public bool isGliding = false;
    public bool isSuperSpeed = false;
    public bool isShrinking = false;
    public bool canUseAbilities = true;
    private bool hasGrappled = false;

    private float defaultSpeed;
    private float originalGravity;

    private Vector2 grappleTarget;
    private Vector2 currentPos;
    private bool shouldGrappleMove = false;
    private float originalCollisionRadius;
    private Vector2 originalBottomOffset;
    private Vector2 originalRightOffset;
    private Vector2 originalLeftOffset;
    private Vector2 originalCapsulesSize;
    private Vector2 originalCapsuleOffset;

    public List<AbilityType> abilities = new List<AbilityType>();

    [Header("Teleport Settings")]
    public float teleportForce = 5f;
    public float teleportDuration = 0.3f;
    public AnimationCurve teleportCurve;
    public ParticleSystem teleportInEffect;
    public ParticleSystem teleportOutEffect;
    private Vector2 teleportPreviewSpot;

    [Header("Super Speed")]
    public ParticleSystem speedParticle;
    public float deaccelerateSpeed = 0.15f;

    [Header("Grapple Settings")]
    public float grappleSpeed = 10f;
    public Vector2 grappleLaunchDirection = Vector2.up;
    public float grappleLaunchForce = 10f;

    [Header("Glide Settings")]
    public float maxGlideTime = 2f;
    private float glideTimer;


    public KeyCode teleportKey = KeyCode.T;
    public KeyCode grappleKey = KeyCode.H;
    public KeyCode superSpeedKey = KeyCode.J;
    public KeyCode glideKey = KeyCode.G;
    public KeyCode shrinkKey = KeyCode.F;
    public KeyCode dashkKey = KeyCode.LeftShift;
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


    string[] keyStrings = new string[]
{
    "Y", "H", "U", "J", "I", "K", "O", "L", "P", "Semicolon", "LeftBracket", "RightBracket", "Quote" };


    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
        anim = GetComponentInChildren<AnimationScript>();
        sr = GetComponent<SpriteRenderer>();
        ghostTrail = FindObjectOfType<GhostTrail>();

        defaultSpeed = movement.speed;
        originalGravity = rb.gravityScale;
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();

        if (capsule != null)
        {
            originalCapsulesSize = capsule.size;
            originalCapsuleOffset = capsule.offset;
        }

        if (collision != null)
        {
            originalCollisionRadius = collision.collisionRadius;
            originalBottomOffset = collision.bottomOffset;
            originalRightOffset = collision.rightOffset;
            originalLeftOffset = collision.leftOffset;
        }

        if (teleportCurve == null)
            teleportCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        glideTimer = maxGlideTime;
    }
    void Update()
    {
        // --- GENERAL MOVEMENT ---
        if (!isSuperSpeed)
            movement.speed = Mathf.Lerp(movement.speed, defaultSpeed, deaccelerateSpeed);

        // Reset glide timer when grounded
        if (collision.onGround)
            glideTimer = maxGlideTime;

        // --- ABILITY INPUTS ---
        if (Input.GetKeyDown(hoverKey))
        {
            StartCoroutine(StopAllMomentum(2f));
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            ResetAbilities();
        }
        // Glide activation
        if (Input.GetKeyDown(glideKey) && glideTimer > 0 && canUseAbilities && HasAbility(AbilityType.Glide))
        {
            Glide();
        }
        if (Input.GetKeyDown(shrinkKey) && HasAbility(AbilityType.Shrink))
        {
            Shrink();
        }

        // Teleport activation  
        if (Input.GetKeyDown(teleportKey) && !isTeleporting && canUseAbilities && HasAbility(AbilityType.Teleport))
        {
            StartCoroutine(TeleportSequence());
        }
        // Grapple activation
        if (Input.GetKeyDown(grappleKey) && canUseAbilities && HasAbility(AbilityType.Grapple))
        {
            GrappleHook();
        }
                // SuperSpeed activation (hold J, works in air too)
                if (Input.GetKey(superSpeedKey) && canUseAbilities && (collision.onGround || isSuperSpeed) && HasAbility(AbilityType.SuperSpeed))
                {
                    SuperSpeed();

                if (!collision.onGround && speedParticle && speedParticle.isPlaying)
                    speedParticle.Stop();

                }
                else
                {
                    isSuperSpeed = false;
                    if (speedParticle && speedParticle.isPlaying)
                        speedParticle.Stop();
                }




            // --- GLIDE LOGIC ---
            if (isGliding && glideTimer > 0)
            {
                rb.gravityScale = 0.5f;
                betterJumping.enabled = false;
                glideTimer -= Time.deltaTime;

                if (glideTimer <= 0)
                    isGliding = false;
            }

            if (!(isGliding || isGrappling) || collision.onGround)
            {
                rb.gravityScale = 3f;
                isGliding = false;
                betterJumping.enabled = true;
            }

            // --- TELEPORT PREVIEW ---
            float x = Input.GetAxis("Horizontal");
            float y = Input.GetAxis("Vertical");
            Vector2 dir = new Vector2(x, y).normalized;
            teleportPreviewSpot = (Vector2)transform.position + dir * teleportForce;

    }

    public Vector2 launchDir;
    void FixedUpdate()
    {
        if (isGrappling && shouldGrappleMove)
        {
            Vector2 toTarget = grappleTarget - rb.position;
            float distance = toTarget.magnitude;
            Vector2 directionToTarget = toTarget.normalized;

            float moveStep = grappleSpeed * Time.fixedDeltaTime;

            if (distance > 0.1f) // move straight toward target
            {
                rb.MovePosition(rb.position + directionToTarget * Mathf.Min(moveStep, distance));
                rb.gravityScale = 0f;
            }
            else // reached target, launch
            {
                // Add horizontal movement toward the target
               // launchDir = new Vector2(test.x, 1f).normalized; // always move horizontally toward target, slight upward
                rb.linearVelocity = test * 50;

                anim.SetBool("isGrappling", false);
                isGrappling = false;
                shouldGrappleMove = false;
                movement.canMove = true;
                rb.gravityScale = 3f;
                betterJumping.enabled = true;
            }
        }

        if (rb.linearVelocity.magnitude > 40f)
            rb.linearVelocity = rb.linearVelocity.normalized * 40f;
    }

    // ========== ABILITY METHODS ==========

    public void Glide()
    {
        Debug.Log("Glide ability activated.");
        isGliding = true;
    }

    public Vector2 test;
    public void GrappleHook()
    {
        Debug.Log("Grapple Hook ability activated.");

        CircleCollider2D grappleRange = GetComponentInChildren<CircleCollider2D>();
        Collider2D[] found = Physics2D.OverlapCircleAll(transform.position, grappleRange.radius);
        List<Collider2D> grapplePoints = new List<Collider2D>();

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

        if (closest == null)
        {
            Debug.LogWarning("No grapple points found nearby!");
            return;
        }

        grappleTarget = closest.transform.position;

        if (closest != null)
        {
            grappleTarget = closest.transform.position;

            anim.Flip(grappleTarget.x < transform.position.x ? -1 : 1);
            movement.canMove = false;
            rb.gravityScale = 0;
            anim.SetTrigger("grappleCast");
            anim.SetBool("isGrappling", true);
            shouldGrappleMove = true;
            isGrappling = true;
            currentPos = rb.position;
            betterJumping.enabled = false;

            SFXManager.Instance.Play(SFXManager.Instance.grappleClip, 1f, 0.95f, 1.05f);

            if (ghostTrail != null)
                ghostTrail.ShowGhost();
        }

        test = grappleTarget - rb.position;
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;
        movement.canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        betterJumping.enabled = false;
        SFXManager.Instance.Play(SFXManager.Instance.teleportOutClip, 1f);

        yield return StartCoroutine(ImplodeEffect());

        if (teleportInEffect)
        {
            ParticleSystem instance = Instantiate(teleportInEffect, transform.position, Quaternion.identity);
            Destroy(instance.gameObject, instance.main.duration + instance.main.startLifetime.constantMax);
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;

        float maxDistance = teleportForce;
        Collider2D myCollider = GetComponent<Collider2D>();

        Vector2 intendedEnd = (Vector2)transform.position + dir * maxDistance;
        float checkRadius = myCollider.bounds.extents.magnitude * 0.9f;
        Collider2D hitAtEnd = Physics2D.OverlapCircle(intendedEnd, checkRadius);

        if (hitAtEnd == null || hitAtEnd == myCollider)
        {
            transform.position = intendedEnd;
        }
        else
        {
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDistance);
            Vector2 farthestHitPoint = Vector2.zero;
            float farthestDistance = -Mathf.Infinity;

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != myCollider)
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
                transform.position = farthestHitPoint + (-dir * (myCollider.bounds.extents.magnitude + 0.1f));
        }

        yield return StartCoroutine(ReformEffect());

        rb.gravityScale = originalGravity;
        betterJumping.enabled = true;
        movement.canMove = true;
        isTeleporting = false;
    }

    IEnumerator ImplodeEffect()
    {
        float duration = teleportDuration / 2f;
        float t = 0;
        Vector3 startScale = transform.localScale;
        SFXManager.Instance.Play(SFXManager.Instance.teleportInClip, 1f);

        if (ghostTrail != null)
            ghostTrail.ShowGhost();

        if (teleportInEffect)
            teleportInEffect.Play();

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float scaleFactor = Mathf.Lerp(1f, 0f, teleportCurve.Evaluate(p));
            transform.localScale = startScale * scaleFactor;

            if (sr) sr.color = new Color(1, 1, 1, Mathf.Lerp(1f, 0.2f, p));
            yield return null;
        }
    }

    IEnumerator ReformEffect()
    {
        float duration = teleportDuration / 2f;
        float t = 0;
        Vector3 endScale = Vector3.one;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = t / duration;
            float scaleFactor = Mathf.Lerp(0f, 1f, teleportCurve.Evaluate(p));
            transform.localScale = endScale * scaleFactor;
            if (sr) sr.color = new Color(1, 1, 1, Mathf.Lerp(0.2f, 1f, p));
            yield return null;
        }

        if (teleportOutEffect)
            teleportOutEffect.Play();
    }

    public void SuperSpeed()
    {
        isSuperSpeed = true;
        movement.speed = defaultSpeed * 3;

        if (collision.onGround && speedParticle && !speedParticle.isPlaying)
            speedParticle.Play();
    }

    public void Shrink()
    {
        Collision coll = GetComponent<Collision>();
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();

        if (isShrinking)
        {
            transform.localScale = Vector3.one;
            isShrinking = false;
            canUseAbilities = true;
            SFXManager.Instance.Play(SFXManager.Instance.unshrinkClip, 1f);

            if (coll != null)
            {
                coll.collisionRadius = originalCollisionRadius;
                coll.bottomOffset = originalBottomOffset;
                coll.rightOffset = originalRightOffset;
                coll.leftOffset = originalLeftOffset;
            }
            if (capsule != null)
            {
                capsule.size = originalCapsulesSize;
                capsule.offset = originalCapsuleOffset;
            }
        }
        else
        {
            isShrinking = true;
            canUseAbilities = false;
            SFXManager.Instance.Play(SFXManager.Instance.shrinkClip, 1f);

            if (coll != null)
            {
                coll.collisionRadius *= 0.5f;
                coll.bottomOffset *= 0.5f;
                coll.rightOffset *= 0.5f;
                coll.leftOffset *= 0.5f;
            }

            if (capsule!= null)
            {
                capsule.size *= 0.5f;
                capsule.offset = new Vector2(capsule.offset.x, 0f);
            }
        }


    }

    public IEnumerator StopAllMomentum(float duration = 2f)
    {
        float originalGravity = rb.gravityScale;
        Vector2 frozenPosition = rb.position;

        // Disable any movement/gravity
        rb.gravityScale = 0;
        rb.linearVelocity = Vector2.zero;
        rb.constraints = RigidbodyConstraints2D.FreezePosition | RigidbodyConstraints2D.FreezeRotation;

        if (betterJumping != null)
            betterJumping.enabled = false;
        if (abilities != null)
            canUseAbilities = false;
        movement.canMove = false;
        movement.isDashing = false;
        movement.wallGrab = false;
        movement.wallSlide = false;

        float timer = 0f;
        while (timer < duration)
        {
            // Forcefully maintain the frozen position
            rb.position = frozenPosition;
            rb.linearVelocity = Vector2.zero;
            timer += Time.deltaTime;
            yield return null;
        }

        // Restore physics
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.gravityScale = originalGravity;

        if (betterJumping != null)
            betterJumping.enabled = true;
        if (abilities != null)
            canUseAbilities = true;
        movement.canMove = true;

        Debug.Log("Player unfrozen.");
    }


    public void AddAbility(AbilityType ability) { 
        abilities.Add(ability);
    }
    public bool HasAbility(AbilityType ability) => abilities.Contains(ability);

    public void SuperSpeedJump(float jumpForce)
    {
        if (collision.onGround)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(teleportPreviewSpot, 0.15f); // 0.15 is the radius of the dot
    }
}
