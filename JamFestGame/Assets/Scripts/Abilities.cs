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
    public bool isGrappling = false;
    public bool isTeleporting = false;
    public bool isGliding = false;
    public bool isSuperSpeed = false;

    private float defaultSpeed;
    private Vector2 grappleTarget;
    private float originalGravity;

    public List<AbilityType> abilities = new List<AbilityType>();

    public float teleportForce = 5f;

    public float teleportDuration = 0.3f;
    public AnimationCurve teleportCurve;

    public ParticleSystem teleportInEffect;
    public ParticleSystem teleportOutEffect;
    public ParticleSystem speedParticle;

    private SpriteRenderer sr;

    private Vector2 teleportPreviewSpot;

    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
        defaultSpeed = movement.speed;
        sr = GetComponent<SpriteRenderer>();

        if (teleportCurve == null)
        {
            teleportCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }

        originalGravity = rb.gravityScale;
        glideTimer = maxGlideTime;
    }

    void Update()
    {
        // Reset glide timer when landing
        if (collision.onGround)
        {
            glideTimer = maxGlideTime;
        }

        // Teleport activation  
        if (Input.GetKeyDown(KeyCode.T) && !isTeleporting)
        {
            StartCoroutine(TeleportSequence());
        }

        // Grapple activation
        if (Input.GetKeyDown(KeyCode.H))
        {
            GrappleHook();
            isGrappling = true;
            FindObjectOfType<GhostTrail>().ShowGhost();
        }

        // SuperSpeed activation (hold J, works in air too)
        if (Input.GetKey(KeyCode.J))
        {
            SuperSpeed();
            // Only play particles if on ground
            if (collision.onGround && speedParticle && !speedParticle.isPlaying)
                speedParticle.Play();
            // Stop particles if not on ground
            if (!collision.onGround && speedParticle && speedParticle.isPlaying)
                speedParticle.Stop();
        }
        else
        {
            movement.speed = defaultSpeed;
            isSuperSpeed = false;
            if (speedParticle && speedParticle.isPlaying)
                speedParticle.Stop();
        }

        // Glide activation
        if (Input.GetKeyDown(KeyCode.G) && glideTimer > 0)
        {
            Glide();
        }

        // Gliding Update Logic
        if (isGliding && glideTimer > 0)
        {
            if (rb != null)
            {
                rb.gravityScale = 0.5f; // Reduce gravity for gliding effect
                betterJumping.enabled = false;
            }
            glideTimer -= Time.deltaTime;
            if (glideTimer <= 0)
            {
                isGliding = false;
            }
        }
        if (!isGliding || collision.onGround)
        {
            if (rb != null)
            {
                rb.gravityScale = 3f; // Reset to normal gravity when not gliding
                isGliding = false;
                betterJumping.enabled = true;
            }
        }

        // Grappling update logic
        if (isGrappling)
        {
            if (grappleTarget != null)
                transform.position = Vector2.Lerp(transform.position, grappleTarget, .03f);
        }
        if (isGrappling && Vector2.Distance(transform.position, grappleTarget) < 0.5f)
        {
            isGrappling = false;
            movement.canMove = true;
            rb.gravityScale = 3;
        }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector2 dir = new Vector2(x, y).normalized;

        teleportPreviewSpot = (Vector2)transform.position + dir * teleportForce;
    }
    public void Glide()
    {
        // Implementation for Glide ability
        Debug.Log("Glide ability activated.");

        if (rb != null)
        {
            isGliding = true;
        }
    }

    public void GrappleHook()
    {
        Debug.Log("Grapple Hook ability activated.");

        CircleCollider2D grappleRange = GetComponentInChildren<CircleCollider2D>();
        Collider2D[] found = Physics2D.OverlapCircleAll(transform.position, grappleRange.radius);
        List<Collider2D> grapplePoints = new List<Collider2D>();

        foreach (var col in found)
        {
            if (col.CompareTag("GrapplePoint"))
            {
                grapplePoints.Add(col);
            }
        }

        Vector2 referencePosition = transform.position;
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

        Vector2 grappleDirection = closest.transform.position;
        if (rb != null)
        {
            if (transform.position.x != grappleDirection.x && transform.position.y != grappleDirection.y)
            {
                movement.canMove = false;
                rb.gravityScale = 0;
                grappleTarget = grappleDirection;
            }

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

        Vector2 intendedEnd = (Vector2) transform.position + dir * maxDistance;

        // Check if there is a collider at the intended end point
        float checkRadius = myCollider.bounds.extents.magnitude * 0.9f; // Slightly less than player size
        Collider2D hitAtEnd = Physics2D.OverlapCircle(intendedEnd, checkRadius);

        if (hitAtEnd == null || hitAtEnd == myCollider)
        {
            // No collider at the end, teleport directly
            transform.position = intendedEnd;
        }
        else
        {
            // Collider at the end, use raycast to find farthest valid point
            Collider2D farthestCollider = null;
            Vector2 farthestHitPoint = Vector2.zero;
            float farthestDistance = -Mathf.Infinity;

            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, dir, maxDistance);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider != null && hit.collider != myCollider)
                {
                    float distance = Vector2.Distance(transform.position, hit.point);
                    if (distance > farthestDistance)
                    {
                        farthestDistance = distance;
                        farthestCollider = hit.collider;
                        farthestHitPoint = hit.point;
                    }
                }
            }

            if (farthestHitPoint != Vector2.zero)
            {
                transform.position = farthestHitPoint + (Vector2)(-dir * (myCollider.bounds.extents.magnitude + 0.1f));
            }
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
        FindObjectOfType<GhostTrail>().ShowGhost();
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

        teleportOutEffect.Play();
    }


    public void SuperSpeed()
    {
        Debug.Log("Super Speed ability activated.");
        isSuperSpeed = true;
        movement.speed = defaultSpeed * 3;
    }

    public void AddAbility(AbilityType ability)
    {
        abilities.Add(ability);
    }
    public bool HasAbility(AbilityType ability)
    {
        return abilities.Contains(ability);
    }

    public float maxGlideTime = 2f; // Duration allowed for gliding (can be set in Inspector)
    private float glideTimer;       // Tracks remaining glide time

    public void SuperSpeedJump(float jumpForce)
    {
        if (collision.onGround)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce); // GOOD: preserves X speed
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(teleportPreviewSpot, 0.15f); // 0.15 is the radius of the dot
    }
}