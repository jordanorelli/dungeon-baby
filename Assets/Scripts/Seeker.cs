using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seeker : MonoBehaviour {
    public LayerMask collisionMask;
    public float range = 3.0f;
    public float maxSpeed = 3.0f; // units per second
    public float acceleration = 3.0f; // increase in velocity per second
    public float drag = 1.0f;

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
        transform.position = transform.position + velocity * Time.deltaTime;
    }

    private void detectPlayer() {
        if (detectPlayer(transform.position + Vector3.left * 1.22f, Vector3.left)) {
            velocity += Vector3.left * acceleration * Time.deltaTime;
        }
        if (detectPlayer(transform.position + Vector3.right * 1.22f, Vector3.right)) {
            velocity += Vector3.right * acceleration * Time.deltaTime;
        }
    }

    private bool detectPlayer(Vector3 start, Vector3 dir) {
        Debug.LogFormat("start: {0} dir: {1}", start, dir);
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
