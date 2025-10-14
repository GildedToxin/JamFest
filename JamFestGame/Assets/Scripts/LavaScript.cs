using UnityEngine;

public class LavaScript : MonoBehaviour
{
    [SerializeField] private GameObject triggerArea;
    private TriggerBox trigger;
    public bool canLava = false;
    public float lavaSpeed = 1f;

    void Start()
    {
        trigger = triggerArea.GetComponent<TriggerBox>();
    }

    void Update()
    {
        if (canLava)
        {
            this.transform.localScale += new Vector3(0, lavaSpeed, 0) * Time.deltaTime;
        }
    }  

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            print("Player lavaed D:");
        }
    }
    public void ResetLava()
    {
        this.transform.localScale = new Vector3(this.transform.localScale.x, 1, this.transform.localScale.z);
    }
}
