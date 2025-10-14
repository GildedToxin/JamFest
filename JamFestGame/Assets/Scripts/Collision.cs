using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Collision : MonoBehaviour
{


    private BoxCollider2D playerCollider;

    [Header("Ground Check Settings")]
    public float groundCheckHeight = 0.1f;
    public float groundCheckWidthMultiplier = 0.8f;
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

    public DefaultSize defaultSize;    

    private void Awake()
    {
        playerCollider = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        defaultSize.GroundColliderPosition = groundCheck.transform.localPosition;
        defaultSize.GroundColliderSize = groundCheck.size;

        defaultSize.PlayerColliderOffest = playerCollider.offset;
        defaultSize.PlayerColliderSize = playerCollider.size;
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
        Bounds bounds = playerCollider.bounds;
        Vector2 center = bounds.center;

        Vector2 rightOrigin = new Vector2(center.x + bounds.extents.x - skinWidth, center.y);
        Vector2 leftOrigin = new Vector2(center.x - bounds.extents.x + skinWidth, center.y);

        onRightWall = Physics2D.Raycast(rightOrigin, Vector2.right, skinWidth * 2f, groundLayer);
        onLeftWall = Physics2D.Raycast(leftOrigin, Vector2.left, skinWidth * 2f, groundLayer);

        onWall = onRightWall || onLeftWall;
    }

    public void ChangeSize(bool isShrinking)
    {
        if (!isShrinking)
        {
            playerCollider.offset = defaultSize.PlayerColliderOffest;
            playerCollider.size = defaultSize.PlayerColliderSize;

            groundCheck.transform.localPosition = defaultSize.GroundColliderPosition;
            groundCheck.size = defaultSize.GroundColliderSize;
        }
        else if (isShrinking)
        {
            playerCollider.offset = new Vector2(playerCollider.offset.x, 0f);
            playerCollider.size *= 0.5f;

            groundCheck.transform.position = new Vector2(transform.position.x, ((Vector2)playerCollider.bounds.center - new Vector2(0, playerCollider.bounds.extents.y)).y);
            groundCheck.size *= 0.95f;
        }
    }

    [System.Serializable]
    public struct DefaultSize
    {
        public Vector2 PlayerColliderOffest;
        public Vector2 PlayerColliderSize;

        public Vector3 GroundColliderPosition;
        public Vector2 GroundColliderSize;
    }
    public struct SmallSize
    {
        public Vector2 PlayerColliderOffest;
        public Vector2 PlayerColliderSize;

        public Vector3 GroundColliderPosition;
        public Vector2 GroundColliderSize;
    }
}


