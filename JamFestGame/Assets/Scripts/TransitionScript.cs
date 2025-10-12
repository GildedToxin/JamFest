using UnityEngine;

public class TransitionScript : MonoBehaviour
{
    [SerializeField] private string transitionSceneName;
    private BoxCollider2D boxCollider;
    private bool canUseDoor = false;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W) && canUseDoor)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(transitionSceneName);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canUseDoor = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canUseDoor = false;
        }
    }
}
