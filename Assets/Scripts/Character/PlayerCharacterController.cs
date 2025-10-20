using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{

    [Header("Character Settings")]
    [SerializeField] private float mass = 70;                 // KG
    [SerializeField] private float walkSpeed = 50f;           // m/s
    [SerializeField] private float sprintSpeed = 100;         // m/s
    [SerializeField] private float jumpHeight = 30f;          // meters
    [SerializeField] private float fixedRotation = 0f;

    [Header("Hover / Spring Settings")]
    [SerializeField] private float desiredHoverHeight = 1.0f; // meters from ground
    [SerializeField] private float springStrength = 50000f;
    [SerializeField] private float damping = 30000;
    [SerializeField] private bool autoCalculateSpring = true;

    [Tooltip("maximum ray length used to detect the ground.")]
    [SerializeField, Range(0f, 5f)] private float springRayLength = 2.0f;

    [Header("Friction")]
    [SerializeField] private float baseFriciton = 0.5f;

    [Header("References")]
    [SerializeField] private PhysicsManager physicsManager;
    [SerializeField] private Transform rayOriginTransform;

    private Rigidbody rb;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool jumpPressed;
    private bool sprintPressed;
    private bool isRagdolled = false;
    private bool isGrounded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        controls = new PlayerControls();
        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Gameplay.Jump.performed += ctx => jumpPressed = true;
        controls.Gameplay.Jump.canceled += ctx => jumpPressed = false;

        controls.Gameplay.Sprint.performed += ctx => sprintPressed = true;
        controls.Gameplay.Sprint.canceled += ctx => sprintPressed = false;

        controls.Gameplay.Ragdoll.performed += ctx => ToggleRagdoll();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        rb.mass = mass;

        if (autoCalculateSpring)
        {
            springStrength = mass * 9.81f * 8f;
            damping = 2 * Mathf.Sqrt(springStrength * mass);
        }


        if (physicsManager != null)
        {
            physicsManager.RegisterRigidbody(rb);
        }
    }

    void FixedUpdate()
    {
        UpdateGroundedStatus();
        CharacterMovement();
        UpwardForce();
        ApplyFriciton();
    }

    #region Movement 
    private void CharacterMovement()
    {
        if (isRagdolled)
        {
            return;
        }

        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        // normalise the vector to prevent travelling faster then intended speeds
        Vector3 movement = (camForward * moveInput.y + camRight * moveInput.x).normalized;

        if (movement.sqrMagnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f));

            float currentSpeed = sprintPressed ? sprintSpeed : walkSpeed;
            Vector3 desiredVelocity = movement * currentSpeed;
            Vector3 velocityChange = desiredVelocity - new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            rb.AddForce(velocityChange * mass, ForceMode.Force);
        }

        if (jumpPressed && isGrounded)
        {
            float jumpVelocity = Mathf.Sqrt(2 * 9.81f * jumpHeight);
            rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);
            jumpPressed = false;
        }
    }
    #endregion

    #region Hover / Spring
    private void UpdateGroundedStatus()
    {
        Vector3 rayOrigin = rayOriginTransform != null ? rayOriginTransform.position : rb.position + Vector3.up;
        isGrounded = Physics.Raycast(rayOrigin, Vector3.down, desiredHoverHeight + 0.1f, LayerMask.GetMask("Ground"));

        Debug.DrawRay(rayOrigin, Vector3.down * (desiredHoverHeight + 0.1f), isGrounded ? Color.green : Color.red);
    }

    private void UpwardForce()
    {
        if (isRagdolled)
        {
            return;
        }

        Vector3 rayOrigin = rayOriginTransform != null ? rayOriginTransform.position : rb.position + Vector3.up * 1f;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, desiredHoverHeight * 2f, groundLayerMask))
        {
            float distance = hit.distance;
            float displacement = desiredHoverHeight - distance;
            float upwardVelocity = Vector3.Dot(rb.linearVelocity, Vector3.up);

            float force = (displacement * springStrength - upwardVelocity * damping);
            rb.AddForce(Vector3.up * force, ForceMode.Force);

            Debug.DrawRay(rayOrigin, Vector3.down * desiredHoverHeight, Color.green);
        }
    }
    #endregion

    #region Friciton
    private void ApplyFriciton()
    {
        if (isRagdolled)
        {
            return;
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        float friction = baseFriciton;

        Vector3 rayOrigin = rayOriginTransform != null ? rayOriginTransform.position : rb.position + Vector3.up * 1f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, desiredHoverHeight * 2f, LayerMask.GetMask("Ground")))
        {
            if (hit.collider.TryGetComponent(out SurfaceProperties surface))
            {
                friction = surface.frictionCoefficient;
            }
        }
        Vector3 frictionForce = -horizontalVelocity * friction * 50f;
        rb.AddForce(frictionForce, ForceMode.Acceleration);

        if (isGrounded && horizontalVelocity.magnitude < 0.1f)
        {
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }
    #endregion

    #region Ragdoll
    private void ToggleRagdoll()
    {
        isRagdolled = !isRagdolled;

        if (isRagdolled)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
        else
        {
            // potential issue here with accessing "transform." (NEED to double check with Stephen!)

            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(fixedRotation, transform.eulerAngles.y, fixedRotation), 0.3f);
        }
    }
    #endregion

    #region Gizmos (Hover Visuliazer)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 rayOrigin = rayOriginTransform != null ? rayOriginTransform.position : transform.position + Vector3.up;

        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * springRayLength);

        Gizmos.color = Color.yellow;
        Vector3 hoverPoint = rayOrigin + Vector3.down * desiredHoverHeight;
        Gizmos.DrawWireSphere(hoverPoint, 0.1f);

        #if UNITY_EDITOR
        UnityEditor.Handles.Label(hoverPoint + Vector3.right * 0.2f, "Hover height");
        #endif
    }

    #endregion
}
