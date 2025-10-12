using UnityEngine;

public class TriggerBox : MonoBehaviour
{
    public bool triggered = false;
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            triggered = true;
        }
    }
}
