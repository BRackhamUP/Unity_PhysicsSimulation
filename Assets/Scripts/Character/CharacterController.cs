using UnityEngine;

public class CharacterController : MonoBehaviour
{//
    private Rigidbody rb;
    [SerializeField] private PhysicsManager physicsManager;
    [SerializeField] public float speed = 50f;
    [SerializeField] public float sprintSpeed = 150;
    [SerializeField] public float jumpHeight = 100f;
    [SerializeField] protected bool isRagdolled = false;
    [SerializeField] protected bool isSprinting = false;
    [SerializeField] protected bool isJumping = false;
    [SerializeField] protected bool isGrounded = false;
    public float fixedRotation = 0f;

    [Header("Hover Settings")]
    [SerializeField] private float desiredHeight = 1.5f;
    [SerializeField] private float springStrength = 5000f;
    [SerializeField] private float damping = 400f;

    [Header("Friction")]
    [SerializeField] private float baseFriciton = 0.5f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        physicsManager = Object.FindFirstObjectByType<PhysicsManager>();

        if (physicsManager != null)
        {
            physicsManager.RegisterRigidbody(rb);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(rb.position, -transform.up, desiredHeight + 0.1f, LayerMask.GetMask("Ground"));
        CharacterMovement();
        UpwardForce();
        ApplyFriciton();
    }
    private void Update()
    {
        HandleRagdoll();
    }

    #region Movement 
    private void CharacterMovement()
    {
        if (isRagdolled)
        {
            return;
        }

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        camForward.y = 0f;
        camRight.y = 0f;

        // normalise the vector to prevent travelling faster then intended speeds
        Vector3 movement = (camForward * vertical + camRight * horizontal).normalized;

        isSprinting = Input.GetKey(KeyCode.LeftShift);
        isJumping = Input.GetKey(KeyCode.Space);

        if (movement.magnitude > 0f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(movement);
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.deltaTime * 10f));

            float currentSpeed = isSprinting ? sprintSpeed : speed;
            rb.AddForce(movement * currentSpeed, ForceMode.Acceleration);
        }

        if (isJumping && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpHeight, ForceMode.Impulse);
        }

    }

    private void UpwardForce()
    {
        if (isRagdolled)
        {
            return;
        }

        RaycastHit hit;
        int groundLayerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(rb.position, -transform.up, out hit, desiredHeight * 2f, groundLayerMask))
        {
            float distance = hit.distance;
            float displacement = desiredHeight - distance;
            float upwardVelocity = Vector3.Dot(rb.linearVelocity, transform.up);

            float force = ((displacement * springStrength) - (upwardVelocity * damping));
            rb.AddForce(transform.up * force, ForceMode.Force);

            Debug.DrawRay(rb.position, -transform.up * desiredHeight, Color.green, 0.1f);
        }
        else
        {
            Debug.DrawRay(rb.position, -transform.up * desiredHeight, Color.red, 0.1f);
        }

    }

    private void ApplyFriciton()
    {
        if (isRagdolled)
        {
            return;
        }

        Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);

        float friction = baseFriciton;
        RaycastHit hit;

        if (Physics.Raycast(rb.position, -transform.up, out hit, desiredHeight * 2f, LayerMask.GetMask("Ground")))
        {
            SurfaceProperties surface = hit.collider.GetComponent<SurfaceProperties>();

            if (surface != null)
            {
                friction = surface.frictionCoefficient;
            }
        }

        if (!isRagdolled)
        {
            Vector3 frictionForce = -horizontalVelocity * friction * 50f;
            rb.AddForce(frictionForce, ForceMode.Acceleration);
        }
    }

    #endregion

    #region Ragdoll
    private void HandleRagdoll()
    {

        if (Input.GetKeyDown(KeyCode.E))
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
                transform.rotation = Quaternion.Euler(fixedRotation, transform.eulerAngles.y, fixedRotation);
            }

        }

    }

    #endregion

}
