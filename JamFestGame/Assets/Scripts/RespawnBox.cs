using UnityEngine;


public class RespawnBox : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.GetComponent<PlayerHealth>())
            return;

        collision.GetComponent<PlayerHealth>().SetRespawnPoint(transform);
    }
}
