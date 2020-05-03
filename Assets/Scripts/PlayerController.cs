using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour {
    public float jumpHeight = 4f;
    public float jumpHorizontalScaling = 1.50f;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6f;
    public float maxFallSpeed = -20f;
    public float timeToMaxRunSpeed = 0.15f;
    public float timeToMaxAirmoveSpeed = 0.025f;
    public float floatTime = 0.05f; // amount of time spent at max jump height before falling again
    public float coyoteTime = 0.1f;

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
        if (Mathf.Abs(input.x) < 0.1) {
            targetX = 0;
        }
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
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*jumpHorizontalScaling;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*jumpHorizontalScaling;
                } else {
                    velocity.x = 0f;
                }
            } else {
                velocity.y = gravity * Time.deltaTime;
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
            }
            break;
        
        case JumpState.Ascending:
            // when ascending, you can alter your forward momentum, but you can't turn around.

            if (velocity.x >= 0) {
                if (targetX >= 0) {
                    // continuing to move in your current direction is a boost
                    velocity.x = Mathf.SmoothDamp(velocity.x, targetX*jumpHorizontalScaling, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                } else {
                    // moving in the opposite direction can slow you down but
                    // not turn you around
                    velocity.x = Mathf.SmoothDamp(velocity.x, Mathf.Clamp(targetX, 0, velocity.x), ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                }
            } else {
                if (targetX <= 0) {
                    // continuing to move in your current direction is a boost
                    velocity.x = Mathf.SmoothDamp(velocity.x, targetX*jumpHorizontalScaling, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                } else {
                    // moving in the opposite direction can slow you down but
                    // not turn you around
                    velocity.x = Mathf.SmoothDamp(velocity.x, Mathf.Clamp(targetX, velocity.x, 0), ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                }
            }

            float n = (Time.time - jumpStateStart) / timeToJumpApex;
            n = Mathf.Clamp(n, 0, 1);

            if (Input.GetButtonDown("Jump") && n >= 0.75f) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*jumpHorizontalScaling;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*jumpHorizontalScaling;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            }

            if (Input.GetButton("Jump")) {
                if (n < 0.4f) {
                    velocity.y = jumpVelocity;
                } else {
                    velocity.y += gravity * Time.deltaTime * n * n;
                }
            } else {
                velocity.y += gravity * Time.deltaTime;
            }

            // if we were rising in the last frame but will be falling in this
            // frame, we should zero out the velocity to float instead.
            if (initialVelocity.y >= 0 && velocity.y <= 0) {
                velocity.y = 0;
                setJumpState(JumpState.Apex);
            }

            break;

        case JumpState.Apex:
            if (Input.GetButtonDown("Jump")) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*jumpHorizontalScaling;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*jumpHorizontalScaling;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
            } else {
                // your horizontal motion at apex is constant throughout
                // velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                float timeFloating = Time.time - jumpStateStart;
                float floatTimeRemaining = floatTime - timeFloating;
                if (floatTimeRemaining < 0) {
                    velocity.y += gravity * -floatTimeRemaining;
                    setJumpState(JumpState.Descending);
                } else {
                    velocity.y = 0;
                }
            }
            break;

        case JumpState.Descending:
            float n2 = (Time.time - jumpStateStart) / timeToJumpApex;
            n2 = Mathf.Clamp(n2, 0, 1);

            // horizontal travel is decreasing when descending, so that you
            // always land vertically. Drag increases as you descend, so that it
            // is 1 at the end of your descent.
            float drag = n2*n2*moveSpeed;
            if (velocity.x >= 0) {
                velocity.x = Mathf.SmoothDamp(velocity.x, Mathf.Clamp(targetX-drag, 0, 1f), ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            } else {
                velocity.x = Mathf.SmoothDamp(velocity.x, Mathf.Clamp(targetX+drag, -1, 0), ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            }
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);

            // fall speed is increasing when descending
            velocity.y += gravity * Time.deltaTime * n2 * n2;
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
                    setJumpState(JumpState.Falling);
                }
                velocity.y += gravity * Time.deltaTime;
            }
            break;
        
        default:
            velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
            velocity.y += gravity * Time.deltaTime;

            // if we were rising in the last frame but will be falling in this
            // frame, we should zero out the velocity to float instead.
            if (initialVelocity.y >= 0 && velocity.y <= 0) {
                velocity.y = 0;
                setJumpState(JumpState.Apex);
            }
            break;
        }

        if (velocity.y < maxFallSpeed) {
            velocity.y = maxFallSpeed;
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
