using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class PlayerController : MonoBehaviour {

    public float gravity = -20f;

    private Vector3 velocity;
    private MoveController moveController;

    void Start() {
        moveController = GetComponent<MoveController>();
    }

    void Update() {
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
