using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// MoveController controls movement for 2D characters
[RequireComponent(typeof(BoxCollider2D))]
public class MoveController : MonoBehaviour
{
    const float skinWidth = 0.015f;

    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;
    public LayerMask collisionMask;
    public CollisionInfo collisions;

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    new BoxCollider2D collider;
    RaycastOrigins raycastOrigins;

    public bool isGrounded {
        get => collisions.below;
    }

    void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        collisions.Reset();

        if (velocity.x != 0) {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0) {
            VerticalCollisions(ref velocity);
        }
        Debug.LogFormat("Moving x: {0} y: {1}", velocity.x, velocity.y);
        transform.Translate(velocity);
    }

    private void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y); // -1 for down, 1 for up
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY > 0) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            if (hit) {
                Debug.LogFormat("Vertical Hit with distance: {0}", hit.distance);
                if (directionY > 0) {
                    collisions.above = true;
                } else {
                    Debug.LogFormat("Hit is below: we are now grounded");
                    collisions.below = true;
                }
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
            }
        }
    }

    private void HorizontalCollisions(ref Vector3 velocity) {
        float directionX = Mathf.Sign(velocity.x); // -1 for left, 1 for right
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;

        for (int i = 0; i < horizontalRayCount; i++) {
            Vector2 rayOrigin = (directionX > 0) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit) {
                if (directionX > 0) {
                    collisions.right = true;
                } else {
                    collisions.left = true;
                }
                velocity.x = (hit.distance - skinWidth) * directionX;
                rayLength = hit.distance;
            }
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2f);

        raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    void CalculateRaySpacing() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

        if (horizontalRayCount < 2) {
            horizontalRayCount = 2;
        }
        if (verticalRayCount < 2) {
            verticalRayCount = 2;
        }

        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    struct RaycastOrigins {
        public Vector2 topLeft;
        public Vector2 topRight;
        public Vector2 bottomLeft;
        public Vector2 bottomRight;
    }

    public struct CollisionInfo {
        public bool above;
        public bool below;
        public bool left;
        public bool right;

        public void Reset() {
            above = false;
            below = false;
            left = false;
            right = false;
        }
    }
}
