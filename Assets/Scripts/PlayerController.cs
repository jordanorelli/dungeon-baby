﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class PlayerController : MonoBehaviour {

    public float jumpHeight = 4f;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6f;

    private float jumpVelocity = 8f;
    private float gravity = -20f;
    private Vector3 velocity;
    private MoveController moveController;

    void Start() {
        moveController = GetComponent<MoveController>();
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update() {
        if (moveController.collisions.below || moveController.collisions.above) {
            velocity.y = 0;
        }
        if (moveController.collisions.left || moveController.collisions.right) {
            velocity.x = 0;
        }

        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (Input.GetKeyDown(KeyCode.Space) && moveController.collisions.below) {
            velocity.y = jumpVelocity;
        }
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
