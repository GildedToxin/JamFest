using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;
    private bool wasOnWall = false;

    [Space]

    [Header("Collision")]

    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset, rightOffset, leftOffset;
    private Color debugCollisionColor = Color.red;

    void Start()
    {
        
    }

    void Update()
    {
        onGround = Physics2D.OverlapCircle((Vector2)transform.position + bottomOffset, collisionRadius, groundLayer);
        onRightWall = Physics2D.OverlapCircle((Vector2)transform.position + rightOffset, collisionRadius, groundLayer);
        onLeftWall = Physics2D.OverlapCircle((Vector2)transform.position + leftOffset, collisionRadius, groundLayer);
        onWall = onRightWall || onLeftWall;

        wallSide = onRightWall ? -1 : 1;

        if (onWall && !wasOnWall)
        {
            SnapToWall();
        }

        wasOnWall = onWall;
    }


    private void SnapToWall()
    {
        Vector2 snapPosition = transform.position;

        if (onRightWall)
        {
            snapPosition.x = transform.position.x + (rightOffset.x - collisionRadius);
        }
        else if (onLeftWall)
        {
            snapPosition.x = transform.position.x + (leftOffset.x + collisionRadius);
        }

        transform.position = snapPosition;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireSphere((Vector2)transform.position  + bottomOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + rightOffset, collisionRadius);
        Gizmos.DrawWireSphere((Vector2)transform.position + leftOffset, collisionRadius);
    }
}
