using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 3;
    private int currentHealth;

    private Rigidbody2D rb;
    private Movement movement;
    private Abilities abilities;
    private Animator animator;

    public float respawnDelay = 1.5f;
    public Transform respawnPoint; // Assign in Inspector

    public bool isLavaLevel;
    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<Movement>();
        abilities = GetComponent<Abilities>();
        animator = GetComponentInChildren<Animator>();
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;

        if (currentHealth > 0)
        {
            animator.SetTrigger("Hurt");
        }
        else
        {
            StartCoroutine(DieAndRespawn());
        }
    }

    private IEnumerator DieAndRespawn()
    {

        float savedGravity = rb.gravityScale;

        animator.SetTrigger("Die");
        SFXManager.Instance.Play(SFXManager.Instance.deathClip);


        if (movement != null)
            movement.canMove = false;
        if (abilities != null)
            abilities.canUseAbilities = false;

        // Freeze physics
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        rb.simulated = false;


        yield return new WaitForSeconds(respawnDelay);
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

        if(isLavaLevel)
            FindAnyObjectByType<LavaScript>().ResetLava();

        animator.Rebind();
        animator.Update(0f);

        // Respawn player
        transform.position = respawnPoint.position;
        currentHealth = maxHealth;

        rb.simulated = true;
        rb.linearVelocity = Vector2.zero;


        rb.gravityScale = savedGravity;


        if (movement != null)
            movement.canMove = true;
        if (abilities != null)
            abilities.canUseAbilities = true;

        animator.SetTrigger("Respawn");
        
    }

    public void SetRespawnPoint(Transform newRepawnPoint)
    {
       respawnPoint = newRepawnPoint;
    }
}
