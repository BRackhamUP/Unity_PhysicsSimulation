using UnityEngine;

public class CharacterController : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] public float speed = 250f;
    [SerializeField] public float jumpHeight = 5f;
    [SerializeField] public bool isRagdolled = false;
    public float fixedRotation = 0f;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        CharacterMovement();
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
            rb.AddForce(movement * speed * Time.fixedDeltaTime);
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
}
