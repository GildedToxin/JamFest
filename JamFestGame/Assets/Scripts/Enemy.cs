using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Vector2[] waypoints;
    public float moveSpeed = 2f;
    public float waitTime = 1f;

    private int currentWaypoint = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
            }
            return;
        }

        Vector2 target = waypoints[currentWaypoint];
        Vector2 current = transform.position;

        // Move towards the target waypoint
        Vector2 newPos = Vector2.MoveTowards(current, target, moveSpeed * Time.deltaTime);
        transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);

        // Check if reached the waypoint
        if (Vector2.Distance(current, target) < 0.05f)
        {
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length; // Loop to start
            isWaiting = true;
            waitTimer = waitTime;
        }
    }
}
