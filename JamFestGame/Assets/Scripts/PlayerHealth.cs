using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Components")]
    private Rigidbody2D rb;
    private Movement movement;
    private Abilities abilities;
    private Animator animator;

    [Header("RespawnSettings")]
    public float respawnDelay = 1.5f;
    public Transform respawnPoint; // Assign in Inspector

    public bool isLavaLevel;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<Movement>();
        abilities = GetComponent<Abilities>();
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(DieAndRespawn());
    }

    private IEnumerator DieAndRespawn()
    {
        float savedGravity = rb.gravityScale;

        animator.SetTrigger("Die");
        SFXManager.Instance.Play(SFXManager.Instance.deathClip);

        movement.canMove = false;
        abilities.CanUseAbilities = false;

        // Freeze physics
        UpdatePlayerPhysics(velocity: Vector2.zero, gravityScale: 0, rbSimulated: false);

        yield return new WaitForSeconds(respawnDelay);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if (isLavaLevel)
            FindAnyObjectByType<LavaScript>().ResetLava();

        animator.Rebind();
        animator.Update(0f);

        // Respawn player
        transform.position = respawnPoint.position;

        UpdatePlayerPhysics(velocity: Vector2.zero, gravityScale: savedGravity, rbSimulated: true);

        movement.canMove = true;
        abilities.CanUseAbilities = true;

        animator.SetTrigger("Respawn");
    }

    public void SetRespawnPoint(Transform newRepawnPoint)
    {
        respawnPoint = newRepawnPoint;
    }

    private void UpdatePlayerPhysics(Vector2 velocity, float gravityScale, bool rbSimulated)
    {
        rb.linearVelocity = velocity;
        rb.gravityScale = gravityScale;
        rb.simulated = rbSimulated;
    }
}