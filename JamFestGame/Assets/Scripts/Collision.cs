using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{


    private BoxCollider2D boxCollider;

    [Header("Layers")]
    [SerializeField] private LayerMask groundLayer;

    [Header("State")]
    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;


    [Header("Ground Check")]
    public BoxCollider2D groundCheck;

    [Header("Ray Settings")]
    public float skinWidth = 0.02f;

    
    // This is bad and should be fixed later
    public Vector3 groundColliderBigPosition;
    public Vector3 groundColliderSmallPosition;

    public Vector2 groundColliderBigSize;
    public Vector2 groundColliderSmallSize;
    //

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        groundColliderBigPosition = groundCheck.transform.localPosition;
        groundColliderBigSize = groundCheck.size;
    }
    void Update()
    {
        CheckWalls();
        CheckGround();

        wallSide = onRightWall ? -1 : 1;
    }

    void CheckGround()
    {
        if (groundCheck != null)
        {
            Vector2 boxCenter = groundCheck.bounds.center;
            Vector2 boxSize = groundCheck.bounds.size;
            onGround = Physics2D.OverlapBox(boxCenter, boxSize, 0f, groundLayer);
        }
    }

    void CheckWalls()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 center = bounds.center;

        Vector2 rightOrigin = new Vector2(center.x + bounds.extents.x - skinWidth, center.y);
        Vector2 leftOrigin = new Vector2(center.x - bounds.extents.x + skinWidth, center.y);

        onRightWall = Physics2D.Raycast(rightOrigin, Vector2.right, skinWidth * 2f, groundLayer);
        onLeftWall = Physics2D.Raycast(leftOrigin, Vector2.left, skinWidth * 2f, groundLayer);

        onWall = onRightWall || onLeftWall;
    }
    /*
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Bounds bounds = GetComponent<BoxCollider2D>().bounds;
        Vector2 center = bounds.center;

        Vector2 rightOrigin = new Vector2(center.x + bounds.extents.x - skinWidth, center.y);
        Vector2 leftOrigin = new Vector2(center.x - bounds.extents.x + skinWidth, center.y);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(rightOrigin, rightOrigin + Vector2.right * skinWidth * 2f);
        Gizmos.DrawLine(leftOrigin, leftOrigin + Vector2.left * skinWidth * 2f);
    } */
}
