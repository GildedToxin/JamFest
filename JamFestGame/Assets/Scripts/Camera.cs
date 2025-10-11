using UnityEngine;

public class Camera : MonoBehaviour
{

    public Transform target;
    public Vector2 offset = new Vector2(0f, 0f);
    public float smoothSpeed = 5f;
    public float followDelay = 0.15f;

    public float verticalDeadZone = 2f;
    public float verticalCatchUpSpeed = 3f;


    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private float delayTimer;

    void LateUpdate()
    {
        if (target == null) return;

        // Horizontal
        float desiredX = target.position.x + offset.x;
        if (Mathf.Abs(desiredX - targetPosition.x) > 0.01f)
            delayTimer += Time.deltaTime;
        else
            delayTimer = 0f;

        if (delayTimer >= followDelay)
            targetPosition.x = desiredX;

        // Vertical
        float desiredY = target.position.y + offset.y;
        float yDistance = desiredY - transform.position.y;

        if (Mathf.Abs(yDistance) > verticalDeadZone)
        {
        
            targetPosition.y = Mathf.Lerp(transform.position.y, desiredY - Mathf.Sign(yDistance) * verticalDeadZone, Time.deltaTime * verticalCatchUpSpeed);
        }
        else
        {
            // Gradually movement
            targetPosition.y = Mathf.Lerp(transform.position.y, desiredY, Time.deltaTime * (verticalCatchUpSpeed * 0.5f));
        }

   
        targetPosition.z = transform.position.z; 
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, 1f / smoothSpeed);
    }
}