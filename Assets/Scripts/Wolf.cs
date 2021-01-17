using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wolf : MonoBehaviour {
    public float sightRange = 3.0f;
    public float sightAngle = 30.0f;
    public int sightRays = 10;

    private Vector3 eyePosition;

    // Start is called before the first frame update
    void Start() {
        eyePosition = new Vector3(-0.5f, 0.5f, 0f);
    }

    // Update is called once per frame
    void Update() {
        for (int i = 0; i < sightRays; i++) {

        }

        Debug.DrawRay(transform.position + eyePosition, sightRange * Vector3.RotateTowards(Vector3.left, Vector3.up, Mathf.Deg2Rad * sightAngle * 0.5f, 0f), Color.red);
        Debug.DrawRay(transform.position + eyePosition, sightRange * Vector3.RotateTowards(Vector3.left, Vector3.down, Mathf.Deg2Rad * sightAngle * 0.5f, 0f), Color.red);
    }
}
