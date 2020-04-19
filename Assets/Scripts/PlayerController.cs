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
    private int frameCount = 0;

    void Start() {
        moveController = GetComponent<MoveController>();
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update() {
        frameCount++;
        Debug.LogFormat("Player Update Start {0} ------------------------", frameCount);
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        Debug.LogFormat("Position x: {0} y: {1}", transform.position.x, transform.position.y);
        Debug.LogFormat("Input x: {0} y: {1}", input.x, input.y);
        float targetX = input.x * moveSpeed;
        Debug.LogFormat("Target dx: {0}", targetX);

        Debug.LogFormat("Above: {0}", moveController.collisions.above);
        if (moveController.collisions.above) {
            velocity.y = 0;
        }

        Debug.LogFormat("Left: {0}", moveController.collisions.left);
        Debug.LogFormat("Right: {0}", moveController.collisions.right);
        if (moveController.collisions.left || moveController.collisions.right) {
            velocity.x = 0;
        }

        Debug.LogFormat("Grounded: {0}", moveController.isGrounded);
        if (moveController.isGrounded) {
            if (Input.GetButtonDown("Jump")) {
                Debug.Log("JUMP");
                velocity.y = jumpVelocity;
            } else {
                velocity.y = gravity * Time.deltaTime;
            }
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
        } else {
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < maxFallSpeed) {
                velocity.y = maxFallSpeed;
            }
        }

        Debug.LogFormat("Velocity x: {0} y: {1}", velocity.x, velocity.y);
        moveController.Move(velocity * Time.deltaTime);
        Debug.LogFormat("Grounded: {0}", moveController.isGrounded);
    }

    void OnDestroy() {
    }

    void OnCollisionEnter(Collision other) {
    }

    void OnTriggerEnter(Collider other) {
    }
}
