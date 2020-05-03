using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TracerDot : MonoBehaviour {
    private float spawned;
    public Color color;

    void Start() {
        spawned = Time.time;
        // color = Color.white;
    }

    void OnDrawGizmos() {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, 0.125f);
    }

    void Update() {
        if (Time.time - spawned > 2) {
            Destroy(gameObject);
        }
    }
}