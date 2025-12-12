using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCharacterController : MonoBehaviour
{
    [Header("Basic")]
    public float mass = 70f;                                       // kg 
    [SerializeField] private float walkSpeed = 3.5f;               // m/s 
    [SerializeField] private float sprintMultiplier = 1.75f;       // sprint = walkSpeed * sprintMultiplier
    [SerializeField] private float jumpHeight = 1.2f;              // meters
    [SerializeField] private float rotationSpeedDegrees = 720f;    // degrees/sec rotation for facing input
    [SerializeField] private bool freezeXZRotation = true;         // keep upright except during ragdoll

    [Header("Acceleration / Control")]
    [SerializeField] private float acceleration = 50f;               // adjust acceleration
    [SerializeField][Range(0f, 1f)] private float airControl = 0.25f;// controlling in air

    [Header("Ground")]
    [SerializeField] private float groundCheckDistance = 1.2f;     // checking how far ground is from the player

    [Header("References")]
    [SerializeField] private Transform rayOriginTransform;
    [SerializeField] private Animator animator;

    /// <summary>
    /// Runtime variables keeping track of movement, input and physics ststes while game is running
    /// </summary>
    Rigidbody rb;
    Vector2 moveInput;
    bool jumpPressed;
    bool sprintPressed;
    bool isGrounded;
    Vector3 groundNormal = Vector3.up;
    bool isRagdolled = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (freezeXZRotation)
            rb.constraints |= RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void OnEnable()
    {
        // connecting up my input manager .
        // https://docs.unity3d.com/Packages/com.unity.inputsystem@1.8/manual/RespondingToActions.html
        var controls = InputManager.controls;

        // subscribe to move/jump/sprint/ragdoll input actions
        controls.CharacterControls.Move.performed += OnMovePerformed;
        controls.CharacterControls.Move.canceled += OnMoveCanceled;
        controls.CharacterControls.JumpBrake.performed += OnJumpBrakePerformed;
        controls.CharacterControls.Sprint.performed += OnSprintPerformed;
        controls.CharacterControls.Sprint.canceled += OnSprintCanceled;
        controls.CharacterControls.Ragdoll.performed += OnRagdollPerformed;

        //enable the action map
        controls.CharacterControls.Enable();
    }

    void OnDisable()
    {
        // unsubscribing from action and disbale action map
        var controls = InputManager.controls;

        controls.CharacterControls.Move.performed -= OnMovePerformed;
        controls.CharacterControls.Move.canceled -= OnMoveCanceled;
        controls.CharacterControls.JumpBrake.performed -= OnJumpBrakePerformed;
        controls.CharacterControls.Sprint.performed -= OnSprintPerformed;
        controls.CharacterControls.Sprint.canceled -= OnSprintCanceled;
        controls.CharacterControls.Ragdoll.performed -= OnRagdollPerformed;

        controls.CharacterControls.Disable();
    }

    // updates to local input states
    private void OnMovePerformed(InputAction.CallbackContext ctx) => moveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled(InputAction.CallbackContext _) => moveInput = Vector2.zero;
    private void OnJumpBrakePerformed(InputAction.CallbackContext _) => jumpPressed = true;
    private void OnSprintPerformed(InputAction.CallbackContext _) => sprintPressed = true;
    private void OnSprintCanceled(InputAction.CallbackContext _) => sprintPressed = false;
    private void OnRagdollPerformed(InputAction.CallbackContext _) => ToggleRagdoll();

    void Start()
    {
        // lock and hide the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // assign animator
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // checking ground applying movement and jump on physics update
        UpdateGroundState();
        HandleMovement();
        HandleJump();
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
        // assign the ray origin to the tranform position
        Vector3 origin = rayOriginTransform.position;
        Debug.DrawRay(origin, Vector3.down * groundCheckDistance, Color.yellow, 0.1f);

        Debug.Log(isGrounded);

        // ground check and storing the normal of the hit
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundCheckDistance, LayerMask.GetMask("Ground")))
        {
            isGrounded = true;
            groundNormal = hit.normal;
        }
        else
        {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }
    #endregion

    #region Movement
    void HandleMovement()
    {
        // no movement control while ragdolled
        if (isRagdolled)
            return;

        // assign cam to transform of main camera
        Transform cam = Camera.main.transform;

        // get forward direciotn of camera and normalise
        Vector3 camForward = cam.forward;
        camForward.y = 0f;
        camForward.Normalize();

        // get right direciton of cam and normalise
        Vector3 camRight = cam.right;
        camRight.y = 0f;
        camRight.Normalize();

        // using the 2d vecotr from the input actions with the camera basis for forward/backward and left/right movement direction
        float inputMagnitude = Mathf.Clamp01(moveInput.magnitude);
        Vector3 desiredDir = camForward * moveInput.y + camRight * moveInput.x;

        if (inputMagnitude > 0.01f)
            desiredDir.Normalize();

        else desiredDir = Vector3.zero;

        // if sprint is pressed mulitply target speed by sprint multiplier if not 1
        // use of ternary operator with '?'
        float targetSpeed = walkSpeed * (sprintPressed ? sprintMultiplier : 1f);

        // update animator with current linear speed for walk to run
        animator.SetFloat("speed", rb.linearVelocity.magnitude);

        // determine the desired velocity based upon being in air or grounded
        Vector3 desiredVelocity;
        if (isGrounded)
        {
            // project movement onto ground plane to follow slopes
            Vector3 moveOnPlane = Vector3.ProjectOnPlane(desiredDir, groundNormal).normalized;
            desiredVelocity = moveOnPlane * (targetSpeed * inputMagnitude);

            if (moveOnPlane.sqrMagnitude > 0.0001f)
                RotateTowards(moveOnPlane);
        }
        else
        {
            // reduce control whilst in the air
            desiredVelocity = desiredDir * (targetSpeed * inputMagnitude * airControl);

            if (desiredDir.sqrMagnitude > 0.0001f)
                RotateTowards(desiredDir);
        }

        // get the current velocity of player
        Vector3 currentVel = rb.linearVelocity;
        Vector3 currentVelOnPlane;

        // if the player is gorunded determine the current velocity with the gorund normal
        if (isGrounded)
        {
            currentVelOnPlane = Vector3.ProjectOnPlane(currentVel, groundNormal);
        }
        // just use current velocity
        else
        {
            currentVelOnPlane = new Vector3(currentVel.x, 0f, currentVel.z);
        }

        // determine the difference between how fast the player is moving and how fast they are supposed to be moving
        Vector3 velocityDelta = desiredVelocity - currentVelOnPlane;

        // convert the needed change into a force
        float response = Mathf.Max(0.0001f, acceleration);
        Vector3 force = velocityDelta * mass * response;

        // when player is in the air, weaken the force by the air control
        if (!isGrounded)
        {
            force *= airControl;
        }

        // apply the force to the player
        rb.AddForce(force, ForceMode.Force);
    }
    #endregion 

    // rotating the character in the direciton of the camera(looking direction)
    void RotateTowards(Vector3 direction)
    {
        Quaternion targetRot = Quaternion.LookRotation(direction);
        Quaternion newRot = Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeedDegrees * Time.fixedDeltaTime);
        rb.MoveRotation(newRot);
    }

    // allowing only to jump when grounded, adding an impulse up
    void HandleJump()
    {
        // cannot jump if ragdolled
        if (isRagdolled)
            return;

        // can only jump when grounded
        if (jumpPressed && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
            jumpPressed = false;
        }
    }

    // toggling the ragdoll just removes all constraints allowing the player to fall 
    void ToggleRagdoll()
    {
        // toggle condition
        isRagdolled = !isRagdolled;

        // remove constraints of the rigidbody
        if (isRagdolled)
            rb.constraints = RigidbodyConstraints.None;

        // reapply the constraints to the rigidbody, allowing the player to continue control
        else
        {
            if (freezeXZRotation)
                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            else
                rb.constraints = RigidbodyConstraints.None;
        }
    }
}
