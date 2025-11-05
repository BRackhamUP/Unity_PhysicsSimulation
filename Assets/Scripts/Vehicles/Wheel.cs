using UnityEngine;

public class Wheel : MonoBehaviour
{
    public Suspension suspension;

    [Header("Wheel Type")]
    public bool isFrontWheel = false;
    public bool isDriven = true;

    [Header("Grip")]
    public float lateralGrip = 5f;
    public float rollingResistance = 0.02f;

    [Header("Brakes")]
    public float maxBrakeForce = 15000f;         // N
    public float maxBrakeDecelleration = 20f;    // m/s^2

    private Rigidbody rb;

    private void Awake()
    {
        // get references to the rigidbody and suspension
        rb = GetComponentInParent<Rigidbody>();
        suspension = GetComponent<Suspension>();
    }

    // called in the vehicle script, utilised in the overridable methoed for updatePhysics
    public void UpdateWheel(float dt)
    {
        suspension.UpdateSuspension(dt, rb);

        // if car is in the air, no need to update wheels
        if (!suspension.grounded) 
            return;

        // retrieve the velocity at each tire's contact point.
        Vector3 velocity = rb.GetPointVelocity(suspension.contact);

        // project the normalised vector for forward and sideways(right) on to the plane to make the wheel local axis stay flat on the roaf surface
        // originally discovered for third-person char, but made more sense here to incorporate for vehicles traversing hills and over bumps
        // https://discussions.unity.com/t/why-use-projectonplane-in-thirdpersoncharacter-script-of-the-standard-assets-example-project/576001
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.normal).normalized;

        // determine how much velocity is sideways or forward
        float sideVel = Vector3.Dot(velocity, right);
        float fwdVel = Vector3.Dot(velocity, forward);

        // use spring force as the load on the wheels
        float wheelSpringLoad =  suspension.load;

        // determine the load scale to prevent high numbers of newtons acting on the grip forces
        float loadScale = wheelSpringLoad * 0.001f;

        Vector3 sideForce = -right * sideVel * lateralGrip * loadScale;
        Vector3 resistForce = -forward * fwdVel * rollingResistance * loadScale;

        rb.AddForceAtPosition(sideForce, suspension.contact, ForceMode.Force);
        rb.AddForceAtPosition(resistForce, suspension.contact, ForceMode.Force);

        Debug.DrawRay(suspension.contact, sideForce * 0.5f, Color.green);
        Debug.DrawRay(suspension.contact, resistForce * 0.5f, Color.red);
    }

    public void ApplyDriveForce(float driveForce)
    {
        // if not grounded not driven wheels or not applying drive force, skip
        if (!suspension.grounded || !isDriven || driveForce == 0) 
            return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;

        float tractionLimit = lateralGrip * suspension.load;

        float clamped = Mathf.Clamp(driveForce, -tractionLimit, tractionLimit);

        rb.AddForceAtPosition(forward * clamped, suspension.contact, ForceMode.Force);
    }

    public void ApplyBrake(float brakeInput)
    {
        if (rb == null || suspension == null) return;
        if (!suspension.grounded || brakeInput <= 0f) return;

        Vector3 vel = rb.GetPointVelocity(suspension.contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        float fwdSpeed = Vector3.Dot(vel, forward);
        if (Mathf.Abs(fwdSpeed) < 0.01f) return;

        float brakeForce = Mathf.Clamp01(brakeInput) * maxBrakeForce;

        if (maxBrakeDecelleration > 0f)
        {
            float decelLimitForce = rb.mass * maxBrakeDecelleration;
            brakeForce = Mathf.Min(brakeForce, decelLimitForce);
        }

        Vector3 force = -Mathf.Sign(fwdSpeed) * forward * brakeForce;
        rb.AddForceAtPosition(force, suspension.contact, ForceMode.Force);

        Debug.DrawRay(suspension.contact, force * 0.0002f, Color.blue);
    }

    public void SetSteerAngle(float angleDegrees)
    {
        if (!isFrontWheel) return;
        transform.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
    }
}

