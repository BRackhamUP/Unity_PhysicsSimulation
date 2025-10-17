using System.Runtime.CompilerServices;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] public float speed = 250f;
    [SerializeField] public float jumpHeight = 5f;
    [SerializeField] public bool isRagdolled = false;
    public float fixedRotation = 0f;
    [SerializeField] private float desiredHeight = 1.5f;
    [SerializeField] private float springStrength = 300f;
    [SerializeField] private float damping = 50f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CharacterMovement();
        UpwardForce();
        ApplyFriciton();
    }

    private void Update()
    {
        ConstrainRotation();
    }

    private void CharacterMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // normalise the vector to prevent travelling faster then intended speeds
        Vector3 movement = new Vector3(horizontal, 0, vertical).normalized;

        if (movement.magnitude > 0f)
        {
            rb.AddForce(movement * speed, ForceMode.Acceleration);
        }

    }

    private void Ragdolled()
    {
        rb.constraints = RigidbodyConstraints.None;
    }


    private void ConstrainRotation()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
            isRagdolled = !isRagdolled;

            if (isRagdolled)
            {
                Ragdolled();
            }
            else
            {
                // potential issue here with accessing "transform." (NEED to double check with Stephen!)

                rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
                transform.rotation = Quaternion.Euler(fixedRotation, transform.eulerAngles.y, fixedRotation);
            }

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

        bool hasHit = Physics.Raycast(rb.position, -transform.up, out hit, 1.5f, groundLayerMask, QueryTriggerInteraction.Collide);

        //debug for checking if it is hitting ground
        Color rayColor = hasHit ? Color.green : Color.red;
        Debug.DrawRay(rb.position, -transform.up * 1.5f, rayColor, 0.1f);

        if (hasHit)
        {
            float distance = hit.distance;
            float displacement = desiredHeight - distance;
            float upwardVelocity = Vector3.Dot(rb.linearVelocity, transform.up);
            float force = ((displacement * springStrength) - (upwardVelocity * damping)) * rb.mass;

            rb.AddForce(transform.up * force, ForceMode.Force);
        }

    }

    private void ApplyFriciton()
    {

        float frictionCoefficient = 10f;

        if (!isRagdolled)
        {
            Vector3 horizontalVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
            Vector3 frictionForce = -horizontalVelocity * frictionCoefficient * rb.mass;
            rb.AddForce(frictionForce, ForceMode.Force);
        }
    }
}
