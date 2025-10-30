using UnityEngine;
public class Wheel : MonoBehaviour 
{ 
    public Suspension suspension; 
    public Engine engine; 
    public bool isFrontWheel; 
    
    public float traction = 1f; 
    public bool isGrounded;
    public float grip = 1.5f; 
    public float frictionCoeff = 150f; 
    private Rigidbody rb; 

    private void Awake() 
    { 
        rb = GetComponentInParent<Rigidbody>();

        if (suspension == null) 
            suspension = GetComponent<Suspension>();

        if (engine == null) 
            engine = GetComponentInParent<Engine>();
    } 
    
    public void UpdateWheel(float deltaTime) 
    { 
        if (rb == null || suspension == null) return;

        suspension.UpdateSuspension(deltaTime, rb); 
        isGrounded = suspension.isGrounded;

        if (!isGrounded) return;

        Vector3 wheelVelocity = rb.GetPointVelocity(transform.position);
        Vector3 forward = transform.forward;
        Vector3 right = transform.right; 

        float forwardVel = Vector3.Dot(wheelVelocity, forward);
        float sidewaysVel = Vector3.Dot(wheelVelocity, right); 

        Vector3 longForce = -forward * forwardVel * frictionCoeff;
        Vector3 latForce = -right * sidewaysVel * frictionCoeff * grip; 
        Vector3 totalFriction = longForce + latForce;

        rb.AddForceAtPosition(totalFriction, transform.position, ForceMode.Force);

        Debug.DrawRay(transform.position, longForce.normalized * 2f, Color.red);
        Debug.DrawRay(transform.position, latForce.normalized * 2f, Color.green);
    }

    public void ApplyTorque(float torque)
    { 
        if (!rb || !suspension || !suspension.isGrounded) 
            return;

        Vector3 driveDir = transform.forward;
        Vector3 driveForce = driveDir * torque * traction;

        rb.AddForceAtPosition(driveForce, suspension.contactPoint, ForceMode.Force);

        Debug.DrawRay(suspension.contactPoint, driveForce.normalized * 2f, Color.blue);
    } 
    
    public void SetSteerAngle(float angleDegrees)
    { 
        if (!isFrontWheel) return;

        transform.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
    }

    public void ApplyBrake(float brakeStrength)
    {
        if (!rb || !suspension || !suspension.isGrounded) return;

        Vector3 wheelVelocity = rb.GetPointVelocity(transform.position);
        Vector3 forward = transform.forward;

        float forwardVelocity = Vector3.Dot(wheelVelocity, forward); 

        if (Mathf.Abs(forwardVelocity) > 0.1f)
        {
            Vector3 brakeForce = -forward * Mathf.Sign(forwardVelocity) * brakeStrength;
            rb.AddForceAtPosition(brakeForce, transform.position, ForceMode.Force);
        }
    }
}