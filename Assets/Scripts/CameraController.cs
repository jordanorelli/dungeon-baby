using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    public Transform player;
    public Camera camera;
    public float maxAcceleration = 1f;
    public float maxVelocity = 10f;

    private Frame frame;

    private Vector3 velocity;
    private Vector3 lastVelocity;
    private Vector3 acceleration;
    private Vector3 lastAcceleration;

    // Start is called before the first frame update
    void Start() {
        camera = gameObject.GetComponent<Camera>();
        velocity.x = 0;
        velocity.y = 0;
        velocity.z = 0;
    }

    void Update() {
    }

    // LateUpdate is called once per frame, after Update
    void LateUpdate() {
        if (!player) {
            return;
        }

        setupFrame();
        Vector3 targetPosition = transform.position;
        BoxCollider2D collider = player.GetComponent<BoxCollider2D>();

        if (collider.bounds.max.x > frame.topRight.x) {
            targetPosition.x = collider.bounds.max.x - frame.topRight.x;
        }
        if (collider.bounds.min.x < frame.topLeft.x) {
            targetPosition.x = collider.bounds.min.x - frame.topLeft.x;
        }
        if (collider.bounds.max.y > frame.topLeft.y) {
            targetPosition.y = collider.bounds.max.y - frame.topLeft.y;
        }
        if (collider.bounds.min.y < frame.bottomLeft.y) {
            targetPosition.y = collider.bounds.min.y - frame.bottomLeft.y;
        }

        
    }

    void OnDrawGizmosSelected() {
        setupFrame();
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

        frame.topLeft = camera.ScreenToWorldPoint(new Vector3(width / 2 - width / 6, height / 2 + height / 3, 1));
        frame.topRight = camera.ScreenToWorldPoint(new Vector3(width / 2 + width / 6, height / 2 + height / 3, 1));
        frame.bottomLeft = camera.ScreenToWorldPoint(new Vector3(width / 2 - width / 6, height / 2 - height / 3, 1));
        frame.bottomRight = camera.ScreenToWorldPoint(new Vector3(width / 2 + width / 6, height / 2 - height / 3, 1));
    }

    struct Frame {
        public Vector3 topLeft;
        public Vector3 topRight;
        public Vector3 bottomLeft;
        public Vector3 bottomRight;
    }
}
