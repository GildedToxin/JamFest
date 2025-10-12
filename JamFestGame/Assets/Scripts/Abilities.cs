using System.Collections;                               
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public enum AbilityType { Dash, DoubleJump, WallJump, WallGrab, Grapple, Glide, Teleport, Shrink, Hover, SuperSpeed, None }
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
    public bool isShrinking = false;

    public bool canUseAbilities = true;

    private float defaultSpeed;
    private Vector2 grappleTarget;
    private float originalGravity;

    private float originalCollisionRadius;
    private Vector2 originalBottomOffset;
    private Vector2 originalRightOffset;
    private Vector2 originalLeftOffset;
    private Vector2 originalCapsulesSize;
    private Vector2 originalCapsuleOffset;

    public List<AbilityType> abilities = new List<AbilityType>();

    public float teleportForce = 5f;

    public float teleportDuration = 0.3f;
    public AnimationCurve teleportCurve;

    public ParticleSystem teleportInEffect;
    public ParticleSystem teleportOutEffect;
    public ParticleSystem speedParticle;

    private SpriteRenderer sr;

    private Vector2 teleportPreviewSpot;

    public float deaccelerateSpeed = 0.15f;

    private bool shouldGrappleMove = false;


    public KeyCode teleportKey = KeyCode.T;
    public KeyCode grappleKey = KeyCode.H;
    public KeyCode superSpeedKey = KeyCode.J;
    public KeyCode glideKey = KeyCode.G;
    public KeyCode shrinkKey = KeyCode.F;

    string[] keyStrings = new string[]
{
    "LeftShift", "Y", "H", "U", "J", "I", "K", "O", "L", "P", "Semicolon", "LeftBracket", "RightBracket", "Quote" };


    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
        defaultSpeed = movement.speed;
        sr = GetComponent<SpriteRenderer>();
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

        originalGravity = rb.gravityScale;
        glideTimer = maxGlideTime;
    }
    void Update()
    {
        if (!isSuperSpeed)
        {////////////////////////////// 7 <--------------- 21
            movement.speed = Mathf.Lerp(movement.speed, defaultSpeed, deaccelerateSpeed);
            print(movement.speed);  
        }
        // Reset glide timer when landing
        if (collision.onGround)
        {
            glideTimer = maxGlideTime;
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            ResetAbilities();
        }
        // Glide activation
        if (Input.GetKeyDown(glideKey) && glideTimer > 0 && canUseAbilities)
        {
            Glide();
        }
        if (Input.GetKeyDown(shrinkKey))
        {
            Shrink();
        }

        // Teleport activation  
        if (Input.GetKeyDown(teleportKey) && !isTeleporting && canUseAbilities)
        {
            StartCoroutine(TeleportSequence());
        }

        // Grapple activation
        if (Input.GetKeyDown(grappleKey) && canUseAbilities)
        {
            GrappleHook();
            isGrappling = true;
            FindObjectOfType<GhostTrail>().ShowGhost();
        }

        // SuperSpeed activation (hold J, works in air too)
        if (Input.GetKey(superSpeedKey) && canUseAbilities)
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
            isSuperSpeed = false;
            if (speedParticle && speedParticle.isPlaying)
                speedParticle.Stop();
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

    public void Shrink()
    {
        Collision coll = GetComponent<Collision>();
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();

        if (isShrinking)
        {
            transform.localScale = Vector3.one;
            isShrinking = false;
            canUseAbilities = true;

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
                    // Add more cases for other abilities with keys as needed
            }

            availableKeys.RemoveAt(index); // Remove used key
        }
    }



    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(teleportPreviewSpot, 0.15f); // 0.15 is the radius of the dot
    }
}