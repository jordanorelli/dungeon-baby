using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraController : MonoBehaviour {
    public Transform player;
    public float maxAcceleration = 1f;
    public float maxVelocity = 10f;
    public Light light;

    private Frame frame;

    // the camera's ideal position, which we will continually move towards but
    // not necessarily snap to (to avoid jerky camera motion)
    private Vector3 targetPosition;

    private Vector3 velocity = Vector3.zero;
    public float smoothTime = 0.0025f;
    // private Vector3 acceleration = Vector3.zero;

    // the values of position, velocity, and acceleration last frame
    // private Vector3 lastPosition = Vector3.zero;
    // private Vector3 lastVelocity = Vector3.zero;
    // private Vector3 lastAcceleration = Vector3.zero;

    private Camera cam;

    // Start is called before the first frame update
    void Start() {
        cam = gameObject.GetComponent<Camera>();
    }

    void Update() {
    }

    // LateUpdate is called once per frame, after Update
    void LateUpdate() {
        if (!player) {
            return;
        }

        setupFrame();
        targetPosition = transform.position;
        BoxCollider2D collider = player.GetComponent<BoxCollider2D>();

        if (collider.bounds.max.x > frame.topRight.x) {
            targetPosition.x += collider.bounds.max.x - frame.topRight.x;
        } else if (collider.bounds.min.x < frame.topLeft.x) {
            targetPosition.x += collider.bounds.min.x - frame.topLeft.x;
        }

        if (collider.bounds.max.y > frame.topLeft.y) {
            targetPosition.y += collider.bounds.max.y - frame.topLeft.y;
        } else if (collider.bounds.min.y < frame.bottomLeft.y) {
            targetPosition.y += collider.bounds.min.y - frame.bottomLeft.y;
        }

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
        light.transform.LookAt(player.transform, Vector3.up);

        // lastPosition = transform.position;
        // lastVelocity = velocity;
        // lastAcceleration = acceleration;
    }

    void OnDrawGizmosSelected() {
        // setupFrame();
        Gizmos.color = Color.white;

        Gizmos.DrawLine(frame.topLeft, frame.topRight);
        Gizmos.DrawLine(frame.topRight, frame.bottomRight);
        Gizmos.DrawLine(frame.bottomRight, frame.bottomLeft);
        Gizmos.DrawLine(frame.bottomLeft, frame.topLeft);

        if (player) {
            BoxCollider2D collider = player.GetComponent<BoxCollider2D>();
            Gizmos.color = Color.red;
            if (collider.bounds.max.x > frame.topRight.x) {
                Gizmos.DrawLine(frame.topRight, frame.bottomRight);
            }
            if (collider.bounds.min.x < frame.topLeft.x) {
                Gizmos.DrawLine(frame.bottomLeft, frame.topLeft);
            }
            if (collider.bounds.max.y > frame.topLeft.y) {
                Gizmos.DrawLine(frame.topLeft, frame.topRight);
            }
            if (collider.bounds.min.y < frame.bottomLeft.y) {
                Gizmos.DrawLine(frame.bottomRight, frame.bottomLeft);
            }
        }
    }

    void setupFrame() {
        int height = Screen.height;
        int width = Screen.width;

        frame.topLeft = cam.ScreenToWorldPoint(new Vector3(width / 2 - width / 9, height / 2 + height / 5, 1));
        frame.topRight = cam.ScreenToWorldPoint(new Vector3(width / 2 + width / 9, height / 2 + height / 5, 1));
        frame.bottomLeft = cam.ScreenToWorldPoint(new Vector3(width / 2 - width / 9, height / 2 - height / 5, 1));
        frame.bottomRight = cam.ScreenToWorldPoint(new Vector3(width / 2 + width / 9, height / 2 - height / 5, 1));
    }

    struct Frame {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
    }
}
