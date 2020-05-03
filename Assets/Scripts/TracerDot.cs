using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerDot : MonoBehaviour {
    private float spawned;

    void Start() {
        spawned = Time.time;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.125f);
    }

    void Update() {
        if (Time.time - spawned > 2) {
            Destroy(gameObject);
        }
    }
}