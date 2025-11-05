using UnityEngine;

public class Wheel : MonoBehaviour
{
    public Suspension suspension;

    [Header("Wheel Type")]
    public bool isFrontWheel = false;
    public bool isDriven = true;

    [Header("Grip")]
    public float lateralGrip = 1.6f;
    public float rollingResistance = 0.015f;

    [Header("Brakes")]
    public float maxBrakeForce = 15000f;         // N
    public float maxBrakeDecelleration = 20f;    // m/s^2 optional cap

    private Rigidbody rb;
    private float baseWheelLoad = 1000f;

    private void Awake()
    {
        // get references to the rigidbody and suspension
        rb = GetComponentInParent<Rigidbody>();
        suspension = GetComponent<Suspension>();

        // using 'var' to infer the variable type of the wheels - https://www.geeksforgeeks.org/c-sharp/var-keyword-in-c-sharp/
        var parentWheels = rb.GetComponentsInChildren<Wheel>();
        
        // determining the wheel count of the vehicle
        int count = Mathf.Max(1, parentWheels.Length);

        // determining the base load of the vehicle spread amongst each wheel
        baseWheelLoad = rb.mass * 9.81f / count;
    }

    // called in the vehicle script, utilising the overridable methoed for updatePhysics
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

        float sideVel = Vector3.Dot(velocity, right);
        float fwdVel = Vector3.Dot(velocity, forward);

        float load = Mathf.Max(baseWheelLoad, suspension.load, 1f);

        Vector3 sideForce = -right * sideVel * lateralGrip * (load * 0.001f); // scaled
        rb.AddForceAtPosition(sideForce, suspension.contact, ForceMode.Force);

        Vector3 resistForce = -forward * fwdVel * rollingResistance * (load * 0.001f);
        rb.AddForceAtPosition(resistForce, suspension.contact, ForceMode.Force);

        Debug.DrawRay(suspension.contact, sideForce * 0.5f, Color.green);
        Debug.DrawRay(suspension.contact, resistForce * 0.5f, Color.red);
    }

    public void ApplyDriveForce(float driveForce)
    {
        if (rb == null || suspension == null) return;
        if (!suspension.grounded || !isDriven || Mathf.Approximately(driveForce, 0f)) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;

        float tractionLimit = lateralGrip * Mathf.Max(1f, suspension.load, baseWheelLoad);

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

