using UnityEngine;

public class PhysicsCharacterController : MonoBehaviour
{
    [Header("Basic")]
    public float mass = 70f;                     // kg 
    [SerializeField] private float walkSpeed = 3.5f;               // m/s 
    [SerializeField] private float sprintMultiplier = 1.75f;       // sprint = walkSpeed * sprintMultiplier
    [SerializeField] private float jumpHeight = 1.2f;              // meters
    [SerializeField] private float rotationSpeedDegrees = 720f;    // degrees/sec rotation for facing input
    [SerializeField] private bool freezeXZRotation = true;         // keep upright except during ragdoll

    [Header("Acceleration / Control")]
    [SerializeField] private float acceleration = 50f;               // adjust acceleration
    [SerializeField][Range(0f, 1f)] private float airControl = 0.25f;// controlling in air

    [Header("Ground & Slope")]
    [SerializeField] private float groundCheckDistance = 1.2f;     // checking how far ground is from the player
    [SerializeField] private float maxGroundAngle = 50f;           // sloope in degrees the player can walk
    [SerializeField] private float slopeSlideMultiplier = 1.0f;    // how much gravity pulls you down a slope

    [Header("Friction / Drag")]
    [SerializeField] private float groundFriction = 8f;           // stops the player faster on ground
    [SerializeField] private float stopThreshold = 0.05f;         // any speed below this is treated as 0 to stop the player

    [Header("References")]
    [SerializeField] private Transform rayOriginTransform;
    [SerializeField] private Animator animator;

    /// <summary>
    /// Runtime variables keeping track of movement, input and physics ststes while game is running
    /// </summary>
    Rigidbody rb;
    PlayerControls controls;
    Vector2 moveInput;
    bool jumpPressed;
    bool sprintPressed;
    bool isGrounded;
    Vector3 groundNormal = Vector3.up;    // attempting to incorporate the Force normal when dealing with slopes
    bool isRagdolled = false;

    void Awake()
    {
        //Check with STEPHEN on uses of rigidbody.Interpolate | mass | constraintss?
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (freezeXZRotation)
        {
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // connecting up my input manager .
        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/manual/RespondingToActions.html
        controls = new PlayerControls();
        controls.Gameplay.Move.performed += context => moveInput = context.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += context => moveInput = Vector2.zero;
        controls.Gameplay.Jump.performed += context => jumpPressed = true;
        controls.Gameplay.Sprint.performed += context => sprintPressed = true;
        controls.Gameplay.Sprint.canceled += context => sprintPressed = false;
        controls.Gameplay.Ragdoll.performed += context => ToggleRagdoll();
    }
    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
    void Start()
    {
        // making sure mass matches with inspector
        rb.mass = mass;

        animator = GetComponent<Animator>();
    }
    void FixedUpdate()
    {
        UpdateGroundState();
        HandleMovement();
        HandleJump();
        HandleGroundStopping();
    }

    #region Ground Detection
    /// <summary>
    /// Shooting a raycast down to see if the player is on the floor
    /// storing the info about the hit like position and the surface normal
    /// check collision with gorund mask
    /// check if player is grounded
    /// 
    /// using ternary operators in places
    /// https://www.w3schools.com/cs/cs_conditions_shorthand.php
    /// </summary>
    void UpdateGroundState()
    {
        Vector3 origin = rayOriginTransform != null ? rayOriginTransform.position : rb.position + Vector3.up * 0.5f;
        Ray ray = new Ray(origin, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit hit, groundCheckDistance, LayerMask.GetMask("Ground")))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }

        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        if (isRagdolled)
        {
            return;
        }

        // use camera transform, if not use players
        Transform cam = Camera.main != null ? Camera.main.transform : transform;

        // get forward direciotn of camera and normalise
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // get right direciton of cam and normalise
        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        // using the 2d vecotr from the input actions for forward/backward and left/right
        Vector3 desiredDir = camForward * moveInput.y + camRight * moveInput.x;
        float inputMagnitude = Mathf.Clamp01(desiredDir.magnitude);              // clamp movement to prevent going faster diagonally

        if (inputMagnitude > 0.01f) // ensure movement magnitude is between 0 and 0.01 to prevent slight jittering with mouse 
        {
            desiredDir.Normalize();
        }
        else
        {
            desiredDir = Vector3.zero;
        }


        float targetSpeed = walkSpeed * (sprintPressed ? sprintMultiplier : 1f);
        animator.SetFloat("speed", rb.linearVelocity.magnitude);

        Vector3 desiredVelocity;
        if (isGrounded)
        {
            Vector3 moveOnPlane = Vector3.ProjectOnPlane(desiredDir, groundNormal).normalized;
            desiredVelocity = moveOnPlane * (targetSpeed * inputMagnitude);

            if (moveOnPlane.sqrMagnitude > 0.0001f)
            {
                RotateTowards(moveOnPlane);
            }

        }
        else
        {
            desiredVelocity = desiredDir * (targetSpeed * inputMagnitude * airControl);
            if (desiredDir.sqrMagnitude > 0.0001f)
            {
                RotateTowards(desiredDir);
            }

        }

        Vector3 currentVel = rb.linearVelocity;
        Vector3 currentVelOnPlane = isGrounded ? Vector3.ProjectOnPlane(currentVel, groundNormal) : new Vector3(currentVel.x, 0f, currentVel.z);

        Vector3 velocityDelta = desiredVelocity - currentVelOnPlane;

        float resp = Mathf.Max(0.0001f, acceleration); 
        Vector3 force = velocityDelta * mass * resp;

        if (!isGrounded)
        {
            force *= airControl;
        }


        rb.AddForce(force, ForceMode.Force);

        if (isGrounded)
        {
            float slopeAngle = Vector3.Angle(groundNormal, Vector3.up);
            if (slopeAngle > maxGroundAngle)
            {
                Vector3 downPlane = Vector3.ProjectOnPlane(Physics.gravity, groundNormal).normalized;
                Vector3 slideForce = downPlane * mass * Physics.gravity.magnitude * slopeSlideMultiplier;
                rb.AddForce(slideForce, ForceMode.Force);
            }
            else
            {
                Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, groundNormal);
                rb.AddForce(downhill * mass * Physics.gravity.magnitude * slopeSlideMultiplier * 0.1f, ForceMode.Force);
            }

            Vector3 frictionForce = -currentVelOnPlane * groundFriction * mass * Time.fixedDeltaTime;
            rb.AddForce(frictionForce, ForceMode.Force);
        }
    }
    #endregion 

    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeedDegrees * Time.fixedDeltaTime);
        rb.MoveRotation(newRot);
    }

    void HandleJump()
    {
        if (isRagdolled)
        {
            return;
        }

        if (jumpPressed && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpHeight);
            Vector3 v = rb.linearVelocity;

            float requiredDeltaV = jumpVelocity - v.y;
            Vector3 impulse = Vector3.up * requiredDeltaV * mass;
            rb.AddForce(impulse, ForceMode.Impulse);
            jumpPressed = false;
        }
    }


    void HandleGroundStopping()
    {
        if (!isGrounded) return;
        Vector3 horizontalVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        if (horizontalVel.magnitude < stopThreshold)
        {
            rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
        }
    }


    void ToggleRagdoll()
    {
        isRagdolled = !isRagdolled;

        if (isRagdolled)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
        else
        {
            if (freezeXZRotation)
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            else
                rb.constraints = RigidbodyConstraints.None;
        }
    }
}
