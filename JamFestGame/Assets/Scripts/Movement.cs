using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using DG.Tweening;
using System.Dynamic;

public class Movement : MonoBehaviour
{
    
    [HideInInspector]
    [Header("Components")]
    public Rigidbody2D rb;
    private AnimationScript anim;
    private Abilities abilities;
    private BetterJumping betterJumping;
    private Collision coll;

    [Space]
    [Header("Stats")]
    public float speed = 10;
    public float jumpForce = 50;
    public float slideSpeed = 5;
    public float wallJumpLerp = 10;
    public float dashSpeed = 20;
    public float dashSpeedBetter = 7;
    public float dashDragTime = 1f;

    [Space]
    [Header("Booleans")]
    public bool canJump;
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;
    public bool pushedWall;
    public bool doubleJumped;
    public bool hasDoubleJump;
    private bool groundTouch;
    private bool hasDashed;

    [Space]
    [Header("Particles")]
    public ParticleSystem dashParticle;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallJumpParticle;
    public ParticleSystem slideParticle;

    [Space]
    public int side = 1;
    private Vector2 inputDirection;

    void Start()
    {
        coll = GetComponent<Collision>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<AnimationScript>();
        abilities = GetComponent<Abilities>();
        betterJumping = GetComponent<BetterJumping>();
    }

    void Update()
    {
        // Read input in Update
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        inputDirection = new Vector2(x, y);

        // Keep non-physics logic here (animation, state changes, etc.)
        anim.SetHorizontalMovement(x, y, rb.linearVelocity.y);

        if (coll.onWall && Input.GetButton("Fire3") && canMove)
        {
            if (side != coll.wallSide)
                anim.Flip(side * -1);
            wallGrab = true;
            wallSlide = false;
        }

        if (Input.GetButtonUp("Fire3") || !coll.onWall || !canMove)
        {
            wallGrab = false;
            wallSlide = false;
        }

        if (coll.onGround && !isDashing)
        {
            wallJumped = false;
            betterJumping.enabled = true;
        }

        if (coll.onWall && !coll.onGround)
        {
            if (x != 0 && !wallGrab)
            {
                wallSlide = true;
                WallSlide();
            }
        }

        if (!coll.onWall || coll.onGround)
            wallSlide = false;

        if (canJump && !coll.onGround && Input.GetKeyDown(abilities.doubleJumpKey) && doubleJumped && !coll.onWall && abilities.CanUseAbilities && abilities.HasAbility(AbilityType.DoubleJump)){
            Jump(Vector2.up, false);
        }

        if (Input.GetButtonDown("Jump"))
        {
            anim.SetTrigger("jump");

            // Only play jump sound if a jump actually happens
            if (coll.onGround || (hasDoubleJump && !coll.onWall))
            {
                Jump(Vector2.up, false);
                SFXManager.Instance.Play(SFXManager.Instance.jumpClip, 1f, 0.95f, 1.05f);
            }
            else if (coll.onWall && !coll.onGround)
            {
                WallJump();
                SFXManager.Instance.Play(SFXManager.Instance.jumpClip, 1f, 0.95f, 1.05f);
            }
        }


        void GroundTouch()

        {
            hasDashed = false;
            isDashing = false;
            side = anim.sr.flipX ? -1 : 1;

            jumpParticle.Play();

            // Play landing sound
            SFXManager.Instance.Play(SFXManager.Instance.landClip, 1f, 0.95f, 1.05f);
        }
        if ((coll.onGround || coll.onWall) && !hasDoubleJump)
        {
            doubleJumped = true;
        }

        if (Input.GetKeyDown(abilities.dashkKey) && !hasDashed && abilities.CanUseAbilities && abilities.HasAbility(AbilityType.Dash))
        {
            float xRaw = Input.GetAxisRaw("Horizontal");
            float yRaw = Input.GetAxisRaw("Vertical");
            if (xRaw != 0 || yRaw != 0)
                Dash(xRaw, yRaw);
        }

        if (coll.onGround && !groundTouch)
        {
            GroundTouch();
            groundTouch = true;
        }

        if (!coll.onGround && groundTouch)
        {
            groundTouch = false;
        }

        WallParticle(y);

        if (wallGrab || wallSlide || !canMove)
            return;

        if (x > 0)
        {
            side = 1;
            anim.Flip(side);
        }
        if (x < 0)
        {
            side = -1;
            anim.Flip(side);
        }
    }

    void FixedUpdate()
    {
        Walk(inputDirection);

        if (wallGrab && !isDashing)
        {
            rb.gravityScale = 0;
            if (inputDirection.x > .2f || inputDirection.x < -.2f)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);

            float speedModifier = inputDirection.y > 0 ? .5f : 1;

            rb.linearVelocity = new Vector2(rb.linearVelocity.x, inputDirection.y * (speed * speedModifier));
        }
        else if (abilities.IsGliding && !coll.onGround)
        {
            rb.gravityScale = 0.5f;
            betterJumping.enabled = false;
        }
        else if (!wallGrab && !abilities.IsGliding && !isDashing)
        {
            rb.gravityScale = 3;
        }
    }

    void GroundTouch()
    {
        hasDashed = false;
        isDashing = false;

        side = anim.sr.flipX ? -1 : 1;

        jumpParticle.Play();
    }

    private void Dash(float x, float y)
    {
        Camera.main.transform.DOComplete();
        Camera.main.transform.DOShakePosition(.2f, .5f, 14, 90, false, true);
        FindAnyObjectByType<RippleEffect>().Emit(Camera.main.WorldToViewportPoint(transform.position));

        hasDashed = true;
        anim.SetTrigger("dash");

        rb.linearVelocity = Vector2.zero;
        Vector2 dir = new Vector2(x, y);
        rb.linearVelocity += dir.normalized * dashSpeed;

        SFXManager.Instance.Play(SFXManager.Instance.dashClip, 1f, 0.95f, 1.05f);

        StartCoroutine(DashWait());
    }


    IEnumerator DashWait()
    {
        FindAnyObjectByType<GhostTrail>().ShowGhost();
        StartCoroutine(GroundDash());
        DOVirtual.Float(dashSpeedBetter, 0, dashDragTime, RigidbodyDrag);

        dashParticle.Play();
        rb.gravityScale = 0;
        GetComponent<BetterJumping>().enabled = false;
        wallJumped = true;
        isDashing = true;

        yield return new WaitForSeconds(.3f);

        dashParticle.Stop();
        rb.gravityScale = 3;
        GetComponent<BetterJumping>().enabled = true;
        wallJumped = false;
        isDashing = false;
    }

    IEnumerator GroundDash()
    {
        yield return new WaitForSeconds(.15f);
        if (coll.onGround)
            hasDashed = false;
    }

    private void WallJump()
    {
        if ((side == 1 && coll.onRightWall) || side == -1 && !coll.onRightWall)
        {
            side *= -1;
            anim.Flip(side);
        }

        StopCoroutine(DisableMovement(0));
        StartCoroutine(DisableMovement(.1f));

        Vector2 wallDir = coll.onRightWall ? Vector2.left : Vector2.right;

       
        Jump((Vector2.up / 1.5f + wallDir / 1.5f), true);
        wallJumped = true;
    }

    private void WallSlide()
    {
        if (coll.wallSide != side)
            anim.Flip(side * -1);

        if (!canMove)
            return;

        bool pushingWall = false;
        if ((rb.linearVelocity.x > 0 && coll.onRightWall) || (rb.linearVelocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.linearVelocity.x;

        rb.linearVelocity = new Vector2(push, -slideSpeed);
    }

    private void Walk(Vector2 dir)
    {
        if (!canMove || wallGrab) return;

        if (!wallJumped)
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, new Vector2(dir.x * speed, rb.linearVelocity.y), 0.2f );
        }
        else
        {
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, new Vector2(dir.x * speed, rb.linearVelocity.y), wallJumpLerp * Time.fixedDeltaTime);
        }
    }

    private void Jump(Vector2 dir, bool wall)
    {
        if (!coll.onGround && !coll.onWall)
            doubleJumped = false;

        slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
        ParticleSystem particle = wall ? wallJumpParticle : jumpParticle;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        rb.linearVelocity += dir * jumpForce;

        particle.Play();
    }

    IEnumerator DisableMovement(float time)
    {
        canMove = false;
        yield return new WaitForSeconds(time);
        canMove = true;
    }

    void RigidbodyDrag(float x)
    {
        rb.linearDamping = x;
    }

    void WallParticle(float vertical)
    {
        var main = slideParticle.main;

        if (wallSlide || (wallGrab && vertical < 0))
        {
            slideParticle.transform.parent.localScale = new Vector3(ParticleSide(), 1, 1);
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = Color.clear;
        }
    }

    int ParticleSide()
    {
        int particleSide = coll.onRightWall ? 1 : -1;
        return particleSide;
    }
}
