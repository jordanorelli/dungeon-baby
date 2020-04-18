using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class PlayerController : MonoBehaviour {
    public float jumpHeight = 4f;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6f;
    public float maxFallSpeed = -20f;
    public float timeToMaxRunSpeed = 0.15f;
    public float timeToMaxAirmoveSpeed = 0.25f;

    private float jumpVelocity = 8f;
    private float gravity = -20f;
    private Vector3 velocity;
    private MoveController moveController;
    private float velocityXSmoothing;

    void Start() {
        moveController = GetComponent<MoveController>();
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetX = input.x * moveSpeed;

        if (moveController.collisions.above) {
            velocity.y = 0;
        }

        if (moveController.collisions.left || moveController.collisions.right) {
            velocity.x = 0;
        }

        if (moveController.isGrounded) {
            if (Input.GetButton("Jump")) {
                velocity.y = jumpVelocity;
            } else {
                velocity.y = 0;
            }
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
        } else {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < maxFallSpeed) {
                velocity.y = maxFallSpeed;
            }
        }

        moveController.Move(velocity * Time.deltaTime);
    }

    void FixedUpdate() {
    }

    void OnDestroy() {
    }

    void OnCollisionEnter(Collision other) {
    }

    void OnTriggerEnter(Collider other) {
    }
}
