using UnityEngine;

public class DamageSource : MonoBehaviour
{
    public int damage = 1;
    public bool destroyOnHit = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger) return;

        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null)
        {
            player.TakeDamage(damage);
            if (destroyOnHit) Destroy(gameObject);
        }
    }
}
