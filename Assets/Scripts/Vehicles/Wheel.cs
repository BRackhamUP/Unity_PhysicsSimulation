using UnityEngine;

[RequireComponent(typeof(Suspension))]
public class Wheel : MonoBehaviour
{
    public Suspension suspension;

    [Header("Wheel Type")]
    public bool isFrontWheel = false;

    [Header("Grip Settings")]
    public float lateralGrip = 1.6f;
    public float rollingResistance = 0.015f;

    [Header("Brakes")]
    public float maxBrakeForce = 8000f;

    private Rigidbody rb;
    private Engine engine;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (suspension == null) suspension = GetComponent<Suspension>();
    }

    public void UpdateWheel(float dt)
    {
        suspension.UpdateSuspension(dt, rb);
        if (!suspension.grounded) return;

        Vector3 vel = rb.GetPointVelocity(suspension.contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.normal).normalized;

        float sideVel = Vector3.Dot(vel, right);
        float fwdVel = Vector3.Dot(vel, forward);

        float load = Mathf.Max(1f, suspension.load);

        Vector3 sideForce = -right * sideVel * lateralGrip * load;
        rb.AddForceAtPosition(sideForce, suspension.contact, ForceMode.Force);

        Vector3 resistForce = -forward * fwdVel * rollingResistance * load;
        rb.AddForceAtPosition(resistForce, suspension.contact, ForceMode.Force);

        Debug.DrawRay(suspension.contact, sideForce * 0.0002f, Color.green);
        Debug.DrawRay(suspension.contact, resistForce * 0.0002f, Color.red);
    }

    public void ApplyDrive(float torque)
    {
        if (!suspension.grounded) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        float driveForce = torque / engine.wheelRadius;

        float maxForce = lateralGrip * Mathf.Max(1f, suspension.load);
        driveForce = Mathf.Clamp(driveForce, -maxForce, maxForce);

        rb.AddForceAtPosition(forward * driveForce, suspension.contact, ForceMode.Force);
    }

    public void ApplyBrake(float input)
    {
        if (!suspension.grounded || input <= 0f) return;

        Vector3 vel = rb.GetPointVelocity(suspension.contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        float speed = Vector3.Dot(vel, forward);
        if (Mathf.Abs(speed) < 0.1f) return;

        float brakeForce = Mathf.Min(input * maxBrakeForce, Mathf.Abs(speed * 200f));
        Vector3 force = -Mathf.Sign(speed) * forward * brakeForce;
        rb.AddForceAtPosition(force, suspension.contact, ForceMode.Force);
    }

    public void SetSteerAngle(float angleDegrees)
    {
        if (!isFrontWheel) return;
        transform.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
    }

    public void SetEngine(Engine eng)
    {
        engine = eng;
    }

    public void SetRigidbody(Rigidbody rbRef)
    {
        rb = rbRef;
    }
}
