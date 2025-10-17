using UnityEngine;

public class CharacterController : MonoBehaviour
{//
    private Rigidbody rb;
    [SerializeField] private PhysicsManager physicsManager;
    [SerializeField] public float speed = 20f;
    [SerializeField] public float jumpHeight = 5f;
    [SerializeField] public bool isRagdolled = false;
    public float fixedRotation = 0f;

    [Header("Hover Settings")]
    [SerializeField] private float desiredHeight = 2f;
    [SerializeField] private float springStrength = 1000f;
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

        // normalise the vector to prevent travelling faster then intended speeds
        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

        if (movement.magnitude > 0f)
        {
            rb.AddForce(movement * speed, ForceMode.Acceleration);
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
