using UnityEngine;
public class Wheel : MonoBehaviour
{
    public Suspension suspension;
    public bool isFrontWheel;
    public float grip = 1.5f;
    public float brakeStrength = 500f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();

        if (suspension == null)
            suspension = GetComponent<Suspension>();
    }

    public void UpdateWheel(float deltaTime)
    {
        if (rb == null || suspension == null)
            return;

        suspension.UpdateSuspension(deltaTime, rb);

        if (suspension.isGrounded == false)
            return;

        // getting the velocity at the wheel 
        Vector3 wheelVelocity = rb.GetPointVelocity(transform.position);
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // splitting the velocity into forward and sideways components
        float forwardVel = Vector3.Dot(wheelVelocity, forward);
        float sidewaysVel = Vector3.Dot(wheelVelocity, right);

        // applying friction
        Vector3 frictionForce = (-forward * forwardVel - right * sidewaysVel) * grip * rb.mass;

        // adding the friciton force
        rb.AddForceAtPosition(frictionForce, transform.position, ForceMode.Force);
    }

    public void ApplyTorque(float torque)
    {
        if (rb == null || suspension == null || suspension.isGrounded == false)
            return;

        // get the drive force direction and apply torque
        Vector3 driveForce = transform.forward * torque;
        rb.AddForceAtPosition(driveForce, suspension.contactPoint, ForceMode.Force);

        Debug.DrawRay(suspension.contactPoint, driveForce.normalized * 2f, Color.blue);
    }

    public void ApplyBrake(float brakeStrength)
    {
        if (rb == null || suspension.isGrounded == false)
            return;

        Vector3 wheelVelocity = rb.GetPointVelocity(transform.position);
        Vector3 forward = transform.forward;
        float speed = Vector3.Dot(wheelVelocity, forward);

        Vector3 brakeForce = -forward * Mathf.Sign(speed) * brakeStrength;
        rb.AddForceAtPosition(brakeForce, transform.position, ForceMode.Force);
    }

    public void SetSteerAngle(float angleDegrees)
    {
        if (!isFrontWheel)
            return;

        // NEEED TO Clarify for setting the rotation. Temporary fix for now. 
        transform.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
    }
}