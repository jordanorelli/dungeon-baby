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

    private float horizontalRaySpacing;
    private float verticalRaySpacing;

    new BoxCollider2D collider;
    RaycastOrigins raycastOrigins;

    void Start() {
        collider = GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
        UpdateRaycastOrigins();
        VerticalCollisions(ref velocity);
        transform.Translate(velocity);
    }

    private void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign(velocity.y); // -1 for down, 1 for up
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++) {
            Vector2 rayOrigin = (directionY > 0) ? raycastOrigins.topLeft : raycastOrigins.bottomLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x);

            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            if (hit) {
                velocity.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;
            }

            Debug.DrawRay(raycastOrigins.bottomLeft + Vector2.right * verticalRaySpacing * i, Vector2.up * -2, Color.red);
        }
    }

    void UpdateRaycastOrigins() {
        Bounds bounds = collider.bounds;
        bounds.Expand(skinWidth * -2);

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
}
