using UnityEngine;

[RequireComponent(typeof(Suspension))]
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
    public float maxBrakeForce = 15000f; // N
    public float maxBrakeDecelleration = 20f;    // m/s^2 optional cap

    private Rigidbody rb;
    private float baselineLoad = 1000f;

    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        if (suspension == null) suspension = GetComponent<Suspension>();

        if (rb != null)
        {
            var parentWheels = rb.GetComponentsInChildren<Wheel>();
            int count = Mathf.Max(1, parentWheels.Length);
            baselineLoad = rb.mass * 9.81f / count;
        }
    }

    public void UpdateWheel(float dt)
    {
        if (rb == null || suspension == null) return;

        suspension.UpdateSuspension(dt, rb);
        if (!suspension.grounded) return;

        Vector3 vel = rb.GetPointVelocity(suspension.contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.normal).normalized;

        float sideVel = Vector3.Dot(vel, right);
        float fwdVel = Vector3.Dot(vel, forward);

        float load = Mathf.Max(baselineLoad, suspension.load, 1f);

        Vector3 sideForce = -right * sideVel * lateralGrip * (load * 0.001f); // scaled
        rb.AddForceAtPosition(sideForce, suspension.contact, ForceMode.Force);

        Vector3 resistForce = -forward * fwdVel * rollingResistance * (load * 0.001f);
        rb.AddForceAtPosition(resistForce, suspension.contact, ForceMode.Force);

        Debug.DrawRay(suspension.contact, sideForce * 0.0002f, Color.green);
        Debug.DrawRay(suspension.contact, resistForce * 0.0002f, Color.red);
    }

    public void ApplyDriveForce(float driveForce)
    {
        if (rb == null || suspension == null) return;
        if (!suspension.grounded || !isDriven || Mathf.Approximately(driveForce, 0f)) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.normal).normalized;

        float tractionLimit = lateralGrip * Mathf.Max(1f, suspension.load, baselineLoad);

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

