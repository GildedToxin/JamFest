using UnityEngine;
using System.Collections.Generic;


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
    private Vector2 grappleTarget;

    public List<AbilityType> abilities = new List<AbilityType>();

    public float teleportForce = 5f;

    void Start()
    {
        movement = GetComponent<Movement>();
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collision>();
        betterJumping = GetComponent<BetterJumping>();
    }
    
    void Update()
    {
        // For testing purposes, activate abilities with key presses
        if (Input.GetKeyDown(KeyCode.G))
        {
            Glide();
            isGliding = true;
        }
        if (Input.GetKeyDown(KeyCode.H))
        {
            GrappleHook();
            isGrappling = true;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Teleport();
            isTeleporting = true;
        }

        // Gliding Update Logic
        if (isGliding)
        {
            // Implement gliding effect, e.g., reduce gravity or control descent
            if (rb != null)
            {
                rb.gravityScale = 0.5f; // Reduce gravity for gliding effect
                betterJumping.enabled = false;
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
            transform.position = Vector2.Lerp(transform.position, grappleTarget, .03f);
        }
        if (isGrappling && Vector2.Distance(transform.position, grappleTarget) < 0.3f)
        {
            isGrappling = false;
            movement.canMove = true;
            rb.gravityScale = 3;
        }
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

        CircleCollider2D grappleRange = GetComponent<CircleCollider2D>();
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
            //rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, grappleDirection * grappleSpeed, .5f);
            if (transform.position.x != grappleDirection.x && transform.position.y != grappleDirection.y)
            {
                movement.canMove = false;
                rb.gravityScale = 0;
                grappleTarget = grappleDirection;
            }
            
        }
    }
    public void Teleport()
    {
        Debug.Log("Teleport ability activated.");

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float xRaw = Input.GetAxisRaw("Horizontal");
        float yRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(x, y);

        Vector2 teleportDirection = dir * teleportForce;

        transform.position = Vector2.Lerp(transform.position, transform.position + (Vector3)teleportDirection, 1f);
        
    }
    public void AddAbility(AbilityType ability)
    {
        abilities.Add(ability);
    }
    public bool HasAbility(AbilityType ability)
    {
        return abilities.Contains(ability);
    }

}
