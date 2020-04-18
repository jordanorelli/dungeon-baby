using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
public class PlayerController : MonoBehaviour {
    private MoveController moveController;

    void Start() {
        moveController = GetComponent<MoveController>();
    }

    void Update() {
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
