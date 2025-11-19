using UnityEngine;

/// <summary>
///  wheel is reading the input from the suspension and measuring the wheels motion at the suspensions contact point
///  and then applying forces at that point.
///  - a sideways force to handle lateral motion
///  - a rolling resistance to bring the vehicle to a natural stop
/// </summary>
public class Wheel : MonoBehaviour
{
    private Suspension suspension;
    private Rigidbody vehicleRigidbody;

    [Header("Wheels")]
    [Tooltip("Assign the wheel mesh and determine the radius, for visual wheel spin")]
    [SerializeField] private Transform WheelMesh;
    [SerializeField] private float WheelRadius;

    [Header("Wheel Type")]
    [Tooltip("Boolean for wheel porpertys like if it is front wheel or driveable")]
    [SerializeField] private bool frontWheel = false;
    [SerializeField] private bool driven = true;

    [Header("Grip")]
    [Tooltip("Grip whilst sliding, lower is easier to slide, higher is more traction")]
    [SerializeField][Range(0f, 1f)] private float grip = 0.8f;
    [Tooltip("Grip when vehicle is stopped, to prevent sliding sideways")]
    [SerializeField][Range(0f, 1f)] private float gripAtStop = 0.9f;
    [Tooltip("Resisting sideways motion, higher values will give better cornering")]
    [SerializeField] private float slideResistance = 3000f;
    [Tooltip("A rolling resistance coefficient to slow down vehicle when no throttle is being applied")]
    [SerializeField] private float slowDownResistance = 0.015f;

    [Header("Brakes")]
    [Tooltip("Brake force wheels apply, higher is stronger braking (per-wheel)")]
    [SerializeField] private float maxBrakeForce = 5000f;
    [Tooltip("Maximum deceleration (m/s^2) requested when brakeInput = 1")]
    [SerializeField] private float maxBrakeDecelleration = 9f;
    [Tooltip("If contact forward speed is below this (m/s) we remove forward velocity gently to avoid jolt")]
    [SerializeField] private float lowSpeedThreshold = 0.3f;

    private int wheelCountCached = 4;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    public bool IsFrontWheel => frontWheel;
    public bool IsDriven => driven;

    private void Awake()
    {
        // get references to the rigidbody and suspension
        vehicleRigidbody = GetComponentInParent<Rigidbody>();
        suspension = GetComponent<Suspension>();
    }

    // called in the individual vehicle script, utilised in the overridable methoed for updatePhysics
    public void UpdateWheel(float deltaTime)
    {
        // update suspension for the contact, normal and load values
        suspension.UpdateSuspension(deltaTime, vehicleRigidbody);

        // if car is in the air, no need to update wheels
        if (!suspension.Grounded)
            return;

        // retrieve the velocity at each tires contact point
        Vector3 contactVelocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);

        // project the normalised vector for forward and sideways(right) on to the plane to make the wheel local axis stay flat on the roaf surface
        // originally discovered for third-person char, but made more sense here to incorporate for vehicles traversing hills and over bumps
        // https://discussions.unity.com/t/why-use-projectonplane-in-thirdpersoncharacter-script-of-the-standard-assets-example-project/576001
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector3.ProjectOnPlane.html
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.Normal).normalized;

        // determine how much velocity is sideways or forward to calculate grip and rolling resistance
        float sideSpeed = Vector3.Dot(contactVelocity, right);
        float forwardSpeed = Vector3.Dot(contactVelocity, forward);

        // use spring force as the load on the wheels, more load for more grip
        float normalForce = Mathf.Max(0f, suspension.Load);

        Vector3 lateralForce = Vector3.zero;

        // get horizontal velcoity and multiply by mass to get a 'force'  (m * g) (need to incorporate my own 'g')
        Vector3 downslope = Vector3.ProjectOnPlane(Physics.gravity, suspension.Normal) * vehicleRigidbody.mass;

        // use the dot to determine how much force is on the "side" axis. should be positive if gravity tries to pull the wheel sideways
        float downslopeSideways = Vector3.Dot(downslope, right);

        // calculate how much sidewasys force is needed to counteract gravity on a slope
        if (Mathf.Abs(sideSpeed) <= lowSpeedThreshold)
        {
            // how much gravity is trying to pull the vehicle
            float needed = -downslopeSideways;
            // max hold force the tire has when nearly stopped
            float maxHold = gripAtStop * normalForce;
            // apply the amount but clamp to prevent exceeding tire grip
            float appliedHold = Mathf.Clamp(needed * 0.98f, -maxHold, maxHold);

            // apply the lateral force (experiencing some bugs where the holding force can pull the vehicle up a slope sideways dependant on it resistance)
            appliedHold += -sideSpeed * slideResistance;
            lateralForce = right * appliedHold;
        }
        else
        {
            // determine a resisting force opposite to sideways movement using sideways velocity
            float resist = -slideResistance * sideSpeed;
            float maxResist = grip * normalForce;

            // clamped by the tire max grip, for drifting and cornering tuning
            resist = Mathf.Clamp(resist, -maxResist, maxResist);
            lateralForce = right * resist;
        }

        Vector3 rollForce = Vector3.zero;

        // apply a force in the opposite direction of the wheels forward to simulate rolling resistance
        // bringing the vehicle to a stop instead of drifitng off
        if (Mathf.Abs(forwardSpeed) > 0.01f)
        {
            float force = -Mathf.Sign(forwardSpeed) * slowDownResistance * normalForce;
            rollForce = forward * force;
        }

        // apply the force at the contact point of each wheel 
        vehicleRigidbody.AddForceAtPosition(lateralForce, suspension.Contact, ForceMode.Force);
        vehicleRigidbody.AddForceAtPosition(rollForce, suspension.Contact, ForceMode.Force);

        if (showDebug == true)
        {
            Debug.DrawRay(suspension.Contact, lateralForce * 0.001f, Color.green);
            Debug.DrawRay(suspension.Contact, rollForce * 0.001f, Color.red);
            Debug.DrawRay(suspension.Contact, suspension.Normal * 0.2f, Color.blue);
        }

        // determine the visual wheel mesh position to sit on the gorund at the suspensino contact and add the radius, update visual
        WheelMesh.position = suspension.Contact + new Vector3(0, WheelRadius, 0);
        RotateWheelMesh();
    }

    /// <summary>
    /// applying engine power in the wheels forward direction
    /// limits by the tires grip to simulate traction
    /// then applys the force at the wheels contact point for realistic push and torque
    /// </summary>
    public void ApplyDriveForce(float driveForce)
    {
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;

        // grip multiplied by the normal spring force to determine the limit
        float limit = grip * Mathf.Max(0f, suspension.Load);

        // traction is determined by clamping driveForce so it never exceeds the tires grip limit
        float applied = Mathf.Clamp(driveForce, -limit, limit);

        vehicleRigidbody.AddForceAtPosition(forward * applied, suspension.Contact, ForceMode.Force);
    }

    public void ApplyBrake(float brakeInput)
    {
        if (!suspension.Grounded || brakeInput <= 0f) return;

        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        Vector3 contactVelocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);
        float forwardSpeed = Vector3.Dot(contactVelocity, forward);
        float absForwardSpeed = Mathf.Abs(forwardSpeed);

        if (absForwardSpeed < lowSpeedThreshold)
        {
            Vector3 worldVel = vehicleRigidbody.linearVelocity;
            float worldForwardVel = Vector3.Dot(worldVel, forward);
            float removeFraction = Mathf.Clamp01(brakeInput) * 0.9f;
            float perWheelRemove = removeFraction / Mathf.Max(1, wheelCountCached);
            Vector3 newVel = worldVel - forward * worldForwardVel * perWheelRemove;
            vehicleRigidbody.linearVelocity = newVel;
            return;
        }

        float desiredDecel = Mathf.Clamp01(brakeInput) * maxBrakeDecelleration;
        float totalBrakeForce = vehicleRigidbody.mass * desiredDecel;

        int wheels = Mathf.Max(1, wheelCountCached);
        float perWheelForce = totalBrakeForce / wheels;
        perWheelForce = Mathf.Min(perWheelForce, maxBrakeForce);

        Vector3 force = -Mathf.Sign(forwardSpeed) * forward * perWheelForce;
        vehicleRigidbody.AddForceAtPosition(force, suspension.Contact, ForceMode.Force);
    }


    /// <summary>
    ///  simple steering that will defo be changed in future
    /// </summary>
    public void SetSteerAngle(float angle)
    {
        if (!frontWheel)
            return;

        transform.localRotation = Quaternion.Euler(0f, angle, 0f);
    }

    /// <summary>
    /// Converting the forward velocity to the wheel spin, speed / circumferance . rotations per second, then apply the roations
    /// </summary>
    public void RotateWheelMesh()
    {
        float wheelCircumference = WheelRadius * Mathf.PI * 2;
        float velocity = Vector3.Dot(vehicleRigidbody.linearVelocity, transform.forward);

        float RotPerSecond = velocity / wheelCircumference;
        WheelMesh.Rotate(Vector3.right, (RotPerSecond * 360) * Time.fixedDeltaTime);
    }
}