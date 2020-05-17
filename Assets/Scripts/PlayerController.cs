﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveController))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour {
    public float jumpHeight = 4f;
    public float maxForwardJumpBoost = 2.50f;
    public float timeToJumpApex = 0.4f;
    public float moveSpeed = 6f;
    public float maxFallSpeed = -20f;
    public float timeToMaxRunSpeed = 0.15f;
    public float timeToMaxAirmoveSpeed = 0.025f;
    public float maxApexTime = 0.05f; // amount of time spent at max jump height before falling again
    public float coyoteTime = 0.1f;
    public int maxJumps = 2;
    public GameObject tracerPrefab;

    // this has to be public to be readable by the display, which is a code
    // smell.
    public JumpState jumpState;
    public JumpDirection jumpDirection;
    private float jumpStateStart;
    private float jumpStartTime;
    private float jumpHeldDuration;
    public int jumpCount;
    private bool wasHoldingJump;
    public float apexTime;

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
        GameObject tracerObj = Instantiate(tracerPrefab, transform.position, Quaternion.identity);
        TracerDot tracer = tracerObj.GetComponent<TracerDot>();

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
            tracer.color = Color.white;

            if (Input.GetButton("Jump")) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*maxForwardJumpBoost;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*maxForwardJumpBoost;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            } else {
                velocity.y = gravity * Time.deltaTime;
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxRunSpeed);
            }
            break;
        
        case JumpState.Ascending:
            tracer.color = Color.green;

            // n starts at 0 and goes to 1 as we ascend
            float n = Mathf.InverseLerp(0, timeToJumpApex, Time.time - jumpStateStart);

            // the drag coefficient gets bigger as we ascend, terminating at 1
            float dragCoefficient = n * n;

            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*maxForwardJumpBoost;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*maxForwardJumpBoost;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            } else if (Input.GetButton("Jump")) {
                float jumpDrag = jumpVelocity * dragCoefficient;
                if (n <= 0.7) {
                    jumpDrag = 0;
                }
                velocity.y = jumpVelocity - jumpDrag;
            } else {
                // if we're not pressing jump any more we add all the gravity
                velocity.y += 4 * gravity * Time.deltaTime;

                // the amount of time spent floating is equal to the amount of
                // time the player held down the jump button.
                if (wasHoldingJump) {
                    apexTime = Time.time - jumpStartTime;
                }
            }

            float forwardBoost = maxForwardJumpBoost - (maxForwardJumpBoost * dragCoefficient);
            forwardBoost = Mathf.Clamp(forwardBoost, 1, maxForwardJumpBoost);

            switch (jumpDirection) {
            case JumpDirection.Neutral:
                // you can wiggle out of a neutral jump in either direction
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;

            case JumpDirection.Left:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX < 0 ? targetX * forwardBoost : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;

            case JumpDirection.Right:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX > 0 ? targetX * forwardBoost : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            }

            // if we were rising in the last frame but will be falling in this
            // frame, we should zero out the velocity to float instead.
            if (initialVelocity.y >= 0 && velocity.y <= 0) {
                velocity.y = 0;
                if (wasHoldingJump && Input.GetButton("Jump")) {
                    apexTime = maxApexTime;
                }
                setJumpState(JumpState.Apex);
            }
            break;

        case JumpState.Apex:
            tracer.color = Color.magenta;

            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*maxForwardJumpBoost;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*maxForwardJumpBoost;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            } else {
                switch (jumpDirection) {
                case JumpDirection.Neutral:
                    velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                    break;
                case JumpDirection.Left:
                    velocity.x = Mathf.SmoothDamp(velocity.x, targetX < 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                    break;
                case JumpDirection.Right:
                    velocity.x = Mathf.SmoothDamp(velocity.x, targetX > 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                    break;
                }

                float timeAtApex = Time.time - jumpStateStart;
                float apexTimeRemaining = apexTime - timeAtApex;
                if (apexTimeRemaining < 0) {
                    velocity.y += gravity * -apexTimeRemaining;
                    setJumpState(JumpState.Descending);
                } else {
                    velocity.y = 0;
                }
            }
            break;

        case JumpState.Descending:
            tracer.color = Color.yellow;

            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*maxForwardJumpBoost;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*maxForwardJumpBoost;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            }

            float n2 = (Time.time - jumpStateStart) / timeToJumpApex;
            n2 = Mathf.Clamp(n2, 0, 1);
            switch (jumpDirection) {
            case JumpDirection.Neutral:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            case JumpDirection.Left:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX < 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            case JumpDirection.Right:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX > 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            }

            // fall speed is increasing when descending
            velocity.y += gravity * Time.deltaTime * (Mathf.Clamp(4 * n2 * n2, 1, 4));
            break;

        case JumpState.CoyoteTime:
            tracer.color = Color.blue;
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
        
        case JumpState.Falling:
            tracer.color = Color.red;

            if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
                velocity.y = jumpVelocity;
                if (input.x >= 0.25f) {
                    velocity.x = moveSpeed*maxForwardJumpBoost;
                } else if (input.x <= -0.25f) {
                    velocity.x = -moveSpeed*maxForwardJumpBoost;
                } else {
                    velocity.x = 0f;
                }
                setJumpState(JumpState.Ascending);
                break;
            }

            switch (jumpDirection) {
            case JumpDirection.Neutral:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            case JumpDirection.Left:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX < 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            case JumpDirection.Right:
                velocity.x = Mathf.SmoothDamp(velocity.x, targetX > 0 ? targetX : 0, ref velocityXSmoothing, timeToMaxAirmoveSpeed);
                break;
            }
            velocity.y += gravity * Time.deltaTime;
            break;

        default:
            throw new System.Exception("bad jump state: " + jumpState);
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
        wasHoldingJump = Input.GetButton("Jump");
    }

    void OnDestroy() {
    }

    void OnCollisionEnter(Collision other) {
    }

    void OnTriggerEnter(Collider other) {
    }

    void setJumpState(JumpState state) {
        if (jumpState != JumpState.Ascending && state == JumpState.Ascending) {
            jumpStartTime = Time.time;
        }
        jumpState = state;
        jumpStateStart = Time.time;
        if (velocity.x == 0) {
            jumpDirection = JumpDirection.Neutral;
        } else if (velocity.x > 0) {
            jumpDirection = JumpDirection.Right;
        } else {
            jumpDirection = JumpDirection.Left;
        }

        if (state == JumpState.Grounded) {
            jumpCount = 0;
        }
        if (state == JumpState.Ascending) {
            jumpCount++;
        }
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

    public enum JumpDirection {
        Neutral,
        Left,
        Right,
    }
}
