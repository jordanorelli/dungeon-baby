using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

[RequireComponent(typeof(MoveController))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour {
    public float jumpHeight = 4f;
    public float maxForwardJumpBoost = 2.50f;
    public float timeToJumpApex = 0.4f;
    public float lastDashTime = 0f;
    public float dashTime = 0.125f;
    public float dashSpeed = 50f;
    public float dashCooldown = 1.25f;
    public float moveSpeed = 6f;
    public float maxFallSpeed = -20f;
    public float timeToMaxRunSpeed = 0.15f;
    public float timeToMaxAirmoveSpeed = 0.025f;
    public float maxApexTime = 0.05f; // amount of time spent at max jump height before falling again
    public float coyoteTime = 0.1f;
    public int maxJumps = 2;
    public GameObject tracerPrefab;
    public GameObject leftDashIndicator;
    public GameObject rightDashIndicator;

    public PlayerControls controls;

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
        // Debug.Log("Creating new controls in Start!");
        // controls = new PlayerControls();
        setJumpState(JumpState.Falling);
        moveController = GetComponent<MoveController>();
        gravity = -(2* jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
    }

    void Awake() {
        Debug.Log("Creating new controls in Awake!");
    }

    void Update() {
        InputActionPhase jumpPhase = InputActionPhase.Waiting;
        bool jumpTriggered = false;
        if (controls == null) {
            Debug.Log("HOW IS GAMEPLAY NULL");
        }
        if (controls.Gameplay.Jump != null) {
            jumpPhase = controls.Gameplay.Jump.phase;
            jumpTriggered = controls.Gameplay.Jump.triggered;
        }
        // Debug.LogFormat("Jump Phase: {0} Triggered: {1}", jumpPhase, jumpTriggered);

        GameObject tracerObj = Instantiate(tracerPrefab, transform.position, Quaternion.identity);
        TracerDot tracer = tracerObj.GetComponent<TracerDot>();

        Vector2 input = controls.Gameplay.Move.ReadValue<Vector2>();
        // Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
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

            if (dash()) {
                break;
            } else if (controls.Gameplay.Jump.phase == InputActionPhase.Started) {
            // } else if (Input.GetButton("Jump")) {
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
        
        case JumpState.Dash:
            tracer.color = Color.red;
            velocity.x = Mathf.SmoothDamp(velocity.x, velocity.x >= 0 ? moveSpeed : -moveSpeed, ref velocityXSmoothing, dashTime);
            velocity.y = 0;
            float timeDashing = Time.time - lastDashTime;
            if (timeDashing >= dashTime) {
                if (moveController.isGrounded) {
                    setJumpState(JumpState.Grounded);
                } else {
                    setJumpState(JumpState.Falling);
                }
            }
            break;

        case JumpState.Ascending:
            tracer.color = Color.green;

            // n starts at 0 and goes to 1 as we ascend
            float n = Mathf.InverseLerp(0, timeToJumpApex, Time.time - jumpStateStart);

            // the drag coefficient gets bigger as we ascend, terminating at 1
            float dragCoefficient = n * n;

            if (dash()) {
                break;
            // } else if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
            } else if (controls.Gameplay.Jump.triggered && jumpCount < maxJumps) {
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
            // } else if (Input.GetButton("Jump")) {
            } else if (controls.Gameplay.Jump.phase == InputActionPhase.Started) {
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
                // if (wasHoldingJump && Input.GetButton("Jump")) {
                if (wasHoldingJump && controls.Gameplay.Jump.phase == InputActionPhase.Started) {
                    apexTime = maxApexTime;
                }
                setJumpState(JumpState.Apex);
            }
            break;

        case JumpState.Apex:
            tracer.color = Color.magenta;

            if (dash()) {
                break;
            // } else if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
            } else if (controls.Gameplay.Jump.triggered && jumpCount < maxJumps) {
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

            // if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
            if (controls.Gameplay.Jump.triggered && jumpCount < maxJumps) {
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

            if (dash()) {
                break;
            // } else if (Input.GetButtonDown("Jump")) {
            } else if (controls.Gameplay.Jump.triggered) {
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
            tracer.color = Color.grey;

            // if (Input.GetButtonDown("Jump") && jumpCount < maxJumps) {
            if (controls.Gameplay.Jump.triggered && jumpCount < maxJumps) {
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
        // wasHoldingJump = Input.GetButton("Jump");
        wasHoldingJump = controls.Gameplay.Jump.phase == InputActionPhase.Started;

        CheckCollisions();
    }

    void CheckCollisions() {
        if (moveController.collisions.below) {
            Crumble crumble = moveController.collisions.below.GetComponent<Crumble>();
            if (crumble) {
                crumble.Hit();
            }
            CollideWith(moveController.collisions.below);
        }
        CollideWith(moveController.collisions.above);
        CollideWith(moveController.collisions.left);
        CollideWith(moveController.collisions.right);
    }

    void CollideWith(Collider2D other) {
        if (other == null) {
            return;
        }
        Debug.Log(other);

        TouchHazard hazard = other.GetComponent<TouchHazard>();
        if (hazard) {
            if (jumpState == JumpState.Dash) {
                Destroy(hazard.gameObject);
            } else {
                SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex ) ;
                // Destroy(gameObject);
            }
        }

        Smashable smashable = other.GetComponent<Smashable>();
        if (smashable) {
            if (jumpState == JumpState.Dash) {
                Destroy(smashable.gameObject);
            }
        }

        Seeker seeker = other.GetComponent<Seeker>();
        if (seeker) {
            SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex ) ;
        }
    }

    public void OnEnable() {
        Debug.Log("Enabling the controls!");
        if (controls == null) {
            controls = new PlayerControls();
        }
        controls.Enable();
        controls.Gameplay.Enable();
        controls.Gameplay.Jump.Enable();
    }

    public void OnDisable() {
        Debug.Log("Disabling the controls!");
        controls.Disable();
        controls.Gameplay.Disable();
        controls.Gameplay.Jump.Disable();
    }

    void OnDestroy() {
    }

    void OnCollisionEnter(Collision other) {
    }

    void OnTriggerEnter(Collider other) {
    }

    bool dash() {
        // if (!Input.GetButtonDown("Fire1")) {
        if (!controls.Gameplay.Dash.triggered) {
            return false;
        }

        if ((Time.time - lastDashTime) < dashCooldown) {
            return false;
        }

        if (velocity.x >= 0) {
            velocity.x = dashSpeed;
        } else {
            velocity.x = -dashSpeed;
        }
        velocity.y = 0;
        lastDashTime = Time.time;
        setJumpState(JumpState.Dash);
        StartCoroutine(dashFade());
        return true;
    }

    public IEnumerator dashFade() {
        Material m = gameObject.GetComponent<MeshRenderer>().materials[0];
        float dt = 0;
        float h = 0;
        float s = 0;
        float v = 0;
        Color.RGBToHSV(m.color, out h, out s, out v);
        float maxSaturation = s;
        while (dt < dashCooldown) {
            s = Mathf.Clamp(dt / dashCooldown, 0f, maxSaturation);
            m.color = Color.HSVToRGB(h, s, v);
            dt += Time.deltaTime;
            yield return true;
        }
    }

    void setJumpState(JumpState state) {
        leftDashIndicator.SetActive(state == JumpState.Dash && velocity.x < 0);
        rightDashIndicator.SetActive(state == JumpState.Dash && velocity.x >= 0);

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
    Grounded   -> Dash       : a normal dash
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

        Dash,
    }

    public enum JumpDirection {
        Neutral,
        Left,
        Right,
    }
}
