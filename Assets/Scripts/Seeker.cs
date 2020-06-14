using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour {
    public LayerMask collisionMask;
    public LayerMask groundingMask;
    public float range = 3.0f;
    public float maxSpeed = 3.0f; // units per second
    public float acceleration = 3.0f; // increase in velocity per second
    public float drag = 1.0f;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;
    private float skinDepth = 0.01f;

    private Vector3 pivot;
    private Vector3 leftMax;
    private Vector3 rightMax;
    private Vector3 velocity;

    void Start() {
        velocity = Vector3.zero;
    }

    // Start is called before the first frame update
    void Awake() {
        pivot = transform.position;
        leftMax = pivot + Vector3.left * range;
        rightMax = pivot + Vector3.right * range;
    }

    // Update is called once per frame
    void Update() {
        detectPlayer();
        velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);
        Vector3 next = transform.position + velocity * Time.deltaTime;

        if (velocity.x >= 0) {
            // if we're moving right, cast a ray from the bottom right corner to make sure we don't go over the ledge
            RaycastHit2D hitRight = Physics2D.Raycast(transform.position + bottomRight + skinDepth * Vector3.up, Vector2.down, Mathf.Infinity, groundingMask);
            RaycastHit2D nextHitRight = Physics2D.Raycast(next + bottomRight + skinDepth * Vector3.up, Vector2.down, Mathf.Infinity, groundingMask);
            hitRight.distance -= skinDepth;
            nextHitRight.distance -= skinDepth;
            // this is gross but because we're only placing blocks in unit
            // increments we can avoid rounding comparison errors by just
            // checking if the difference between this height and the next
            // height is less than half our minimum delta that we use in the
            // levels. This is faster than figuring out a real solution.
            if (Mathf.Abs(nextHitRight.distance - hitRight.distance) < 0.5f) {
                transform.position = next;
            } else {
                velocity.x = 0;
            }
        } else {
            // if we're moving left, cast a ray from the bottom left corner to make sure we don't go over the ledge
            RaycastHit2D hitLeft = Physics2D.Raycast(transform.position + bottomLeft + skinDepth * Vector3.up, Vector2.down, Mathf.Infinity, groundingMask);
            RaycastHit2D nextHitLeft = Physics2D.Raycast(next + bottomLeft + skinDepth * Vector3.up, Vector2.down, Mathf.Infinity, groundingMask);
            hitLeft.distance -= skinDepth;
            nextHitLeft.distance -= skinDepth;
            if (Mathf.Abs(hitLeft.distance - nextHitLeft.distance) < 0.5f) {
                transform.position = next;
            } else {
                velocity.x = 0;
            }
        }
    }

    private void detectPlayer() {
        bool detected = false;
        if (detectPlayer(transform.position + Vector3.left * 1.22f, Vector3.left)) {
            velocity += Vector3.left * acceleration * Time.deltaTime;
            detected = true;
        }
        if (detectPlayer(transform.position + Vector3.right * 1.22f, Vector3.right)) {
            velocity += Vector3.right * acceleration * Time.deltaTime;
            detected = true;
        }
        if (!detected) {
            velocity.x = 0;
        }
    }

    private bool detectPlayer(Vector3 start, Vector3 dir) {
        RaycastHit2D hit = Physics2D.Raycast(start, dir, range, collisionMask);
        if (hit) {
            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Player")) {
                Debug.DrawRay(start, dir * hit.distance, Color.red);
                return true;
            } else {
                Debug.DrawRay(start, dir * hit.distance, Color.green);
                if (dir.x >= 0) {
                    velocity.x = Mathf.Clamp(velocity.x, -Mathf.Infinity, hit.distance / Time.deltaTime);
                } else {
                    velocity.x = Mathf.Clamp(velocity.x, -hit.distance / Time.deltaTime, Mathf.Infinity);
                }
            }
        } else {
            Debug.DrawRay(start, dir * range, Color.gray);
        }
        return false;
    }
}
