using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour {
    public float jumpHeight = 4f;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6f;
    public float maxFallSpeed = -20f;
    public float timeToMaxRunSpeed = 0.15f;
    public float timeToMaxAirmoveSpeed = 0.25f;
    public float floatTime = 0.025f; // amount of time spent at max jump height before falling again
    public float coyoteTime = 0.025f;

    // this has to be public to be readable by the display, which is a code
    // smell.
    public JumpState jumpState;
    private float jumpStateStart;

    private float jumpVelocity = 8f;
    private float gravity = -20f;
    private Vector3 velocity;
    private MoveController moveController;
    private float velocityXSmoothing;

    void Start() {
        setJumpState(JumpState.Falling);
        moveController = GetComponent<MoveController>();
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Update() {
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetX = input.x * moveSpeed;
        Vector3 initialVelocity = velocity;

        if (moveController.collisions.above) {
            velocity.y = 0;
        }

        if (moveController.collisions.left || moveController.collisions.right) {
            velocity.x = 0;
        }

        switch (jumpState) {
        case JumpState.Grounded:
            if (Input.GetButtonDown("Jump")) {
                setJumpState(JumpState.Ascending);
                velocity.y = jumpVelocity;
            } else {
                velocity.y = gravity * Time.deltaTime;
            }
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
            break;
        
        // case JumpState.Ascending:
        //     velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
        //     velocity.y += gravity * Time.deltaTime;
        //     if (velocity.y < maxFallSpeed) {
        //         velocity.y = maxFallSpeed;
        //     }

        //     // if we were rising in the last frame but will be falling in this
        //     // frame, we should zero out the velocity to float instead.
        //     if (initialVelocity.y >= 0 && velocity.y <= 0) {
        //         velocity.y = 0;
        //         setJumpState(JumpState.Apex);
        //     }
        //     break;

        case JumpState.Apex:
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            float timeFloating = Time.time - jumpStateStart;
            float floatTimeRemaining = floatTime - timeFloating;
            if (floatTimeRemaining < 0) {
                velocity.y += gravity * -floatTimeRemaining;
                setJumpState(JumpState.Descending);
            } else {
                velocity.y = 0;
            }
            break;

        case JumpState.CoyoteTime:
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
            if (Input.GetButtonDown("Jump")) {
                setJumpState(JumpState.Ascending);
                velocity.y = jumpVelocity;
            } else {
                float elapsedCoyoteTime = Time.time - jumpStateStart;
                float coyoteTimeRemaining =  coyoteTime - elapsedCoyoteTime;
                if (coyoteTimeRemaining < 0) {
                    velocity.y += gravity * -coyoteTimeRemaining;
                    setJumpState(JumpState.Falling);
                } else {
                    velocity.y = 0;
                }
            }
            break;
        
        default:
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            velocity.y += gravity * Time.deltaTime;
            if (velocity.y < maxFallSpeed) {
                velocity.y = maxFallSpeed;
            }

            // if we were rising in the last frame but will be falling in this
            // frame, we should zero out the velocity to float instead.
            if (initialVelocity.y >= 0 && velocity.y <= 0) {
                velocity.y = 0;
                setJumpState(JumpState.Apex);
            }
            break;
        }

        moveController.Move(velocity * Time.deltaTime);
        if (jumpState == JumpState.Grounded && !moveController.isGrounded) {
            setJumpState(JumpState.CoyoteTime);
        }
        if (jumpState != JumpState.Grounded && moveController.isGrounded) {
            setJumpState(JumpState.Grounded);
        }
    }

    void OnDestroy() {
    }

    void OnCollisionEnter(Collision other) {
    }

    void OnTriggerEnter(Collider other) {
    }

    void setJumpState(JumpState state) {
        jumpState = state;
        jumpStateStart = Time.time;
    }

    /*

    Possible JumpState transitions:

    Grounded   -> Ascending  : a normal jump
    Grounded   -> CoyoteTime : player has walked off ledge
    CoyoteTime -> Ascending  : player has jumped after leaving a ledge
    CoyoteTime -> Falling    : player has walked off of a ledge and is now falling
    Ascending  -> Apex       : player has reached the top of their jump normally
    Apex       -> Descending : player has reached the top of their jump and is now falling

    */
    public enum JumpState {
        // The player is on the ground
        Grounded,

        // The player has not jumped; they have walked off of a platform without
        // jumping
        CoyoteTime,

        // The player is rising in a jump
        Ascending,

        // The player has reached the apex of their jump, where they will
        // briefly remain to give a feeling of lightness
        Apex,

        // The player, having jumped, is in a controlled descent following the
        // jump
        Descending,

        // The player is descending but without control; they are falling but
        // did not initially jump.
        Falling,
    }
}
