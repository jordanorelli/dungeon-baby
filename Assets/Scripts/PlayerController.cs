using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class PlayerController : MonoBehaviour {

    public float gravity = -20f;
    public float moveSpeed = 6f;

    private Vector3 velocity;
    private MoveController moveController;

    void Start() {
        moveController = GetComponent<MoveController>();
    }

    void Update() {
        if (moveController.collisions.below || moveController.collisions.above) {
            velocity.y = 0;
        }
        if (moveController.collisions.left || moveController.collisions.right) {
            velocity.x = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        velocity.x = input.x * moveSpeed;
        velocity.y += gravity * Time.deltaTime;
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
