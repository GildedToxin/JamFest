using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AbilityType { Dash, DoubleJump, WallJump, WallGrab, Grapple, Glide, Teleport, Shrink, Hover, None }

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

    private float defaultSpeed;
    private float originalGravity;

    private Vector2 grappleTarget;
    private bool shouldGrappleMove = false;

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
    public float grappleSpeed = 15f;
    public Vector2 grappleLaunchDirection = Vector2.up;
    public float grappleLaunchForce = 10f;

    [Header("Glide Settings")]
    public float maxGlideTime = 2f;
    private float glideTimer;

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
        if (Input.GetKeyDown(KeyCode.T) && !isTeleporting && canUseAbilities)
            StartCoroutine(TeleportSequence());

        if (Input.GetKeyDown(KeyCode.H) && canUseAbilities)
            GrappleHook();

        if (Input.GetKey(KeyCode.J) && canUseAbilities)
            SuperSpeed();
        else
        {
            isSuperSpeed = false;
            if (speedParticle && speedParticle.isPlaying)
                speedParticle.Stop();
        }

        if (Input.GetKeyDown(KeyCode.G) && glideTimer > 0 && canUseAbilities)
            Glide();

        if (Input.GetKeyDown(KeyCode.F))
            Shrink();

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

    void FixedUpdate()
    {
        // --- GRAPPLE MOVEMENT ---
        if (isGrappling && shouldGrappleMove)
        {
            rb.gravityScale = 0;
            rb.MovePosition(Vector2.Lerp(rb.position, grappleTarget, 0.1f));

            if (Vector2.Distance(rb.position, grappleTarget) < 1f)
            {
                anim.SetBool("isGrappling", false);
                isGrappling = false;
                shouldGrappleMove = false;
                movement.canMove = true;
                rb.gravityScale = 3f;
            }
        }
    }

    // ========== ABILITY METHODS ==========

    public void Glide()
    {
        Debug.Log("Glide ability activated.");
        isGliding = true;
    }

    public void GrappleHook()
    {
        Debug.Log("Grapple Hook ability activated.");

        CircleCollider2D grappleRange = GetComponent<CircleCollider2D>();
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

        if (rb != null)
        {
            anim.Flip(grappleTarget.x < transform.position.x ? -1 : 1);
            movement.canMove = false;
            rb.gravityScale = 0;
            anim.SetTrigger("grappleCast");
            anim.SetBool("isGrappling", true);
            shouldGrappleMove = true;
            isGrappling = true;

            if (ghostTrail != null)
                ghostTrail.ShowGhost();
        }
    }

    IEnumerator TeleportSequence()
    {
        isTeleporting = true;
        movement.canMove = false;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        betterJumping.enabled = false;

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
        if (transform.localScale == new Vector3(0.5f, 0.5f, 1f))
        {
            canUseAbilities = true;
            isShrinking = false;
            transform.localScale = Vector3.one;
            return;
        }

        canUseAbilities = false;
        isShrinking = true;
        transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    public void AddAbility(AbilityType ability) => abilities.Add(ability);
    public bool HasAbility(AbilityType ability) => abilities.Contains(ability);

    public void SuperSpeedJump(float jumpForce)
    {
        if (collision.onGround)
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }
}
