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
    [SerializeField] private Transform WheelMesh;
    [SerializeField] private float WheelRadius;


    [Header("Wheel Type")]
    [SerializeField] private bool frontWheel = false;
    [SerializeField] private bool driven = true; // going to turn into an enum for front rear and all wheel drive

    [Header("Grip")]
    [SerializeField][Range(0, 1)] private float grip = 0.8f;                
    [SerializeField][Range(0, 1)] private float gripAtStop = 0.9f;
    [SerializeField] private float slideResistance = 3000f;         //N per m/s  converting sideways speed into resisting force
    [SerializeField] private float slowDownResistance = 0.015f;     // rolling resistance coefficient

    // going to be changed
    [Header("Brakes & engine")]
    [SerializeField] private float maxBrakeForce = 15000f;          // N
    [SerializeField] private float maxBrakeDecelleration = 9f;      // m/s^2

    [SerializeField] private float almostStoppedSideSpeed = 0.05f;          // the speed in m/s to be considered not moving
    [SerializeField][Range(0.8f, 1f)] private float holdReduction = 0.98f;  // 


    // public accessible properites other scripts need to access
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
        Vector3 velocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);

        // project the normalised vector for forward and sideways(right) on to the plane to make the wheel local axis stay flat on the roaf surface
        // originally discovered for third-person char, but made more sense here to incorporate for vehicles traversing hills and over bumps
        // https://discussions.unity.com/t/why-use-projectonplane-in-thirdpersoncharacter-script-of-the-standard-assets-example-project/576001
        // https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Vector3.ProjectOnPlane.html
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.Normal).normalized;

        // determine how much velocity is sideways or forward
        float sideSpeed = Vector3.Dot(velocity, right);
        float forwardSpeed = Vector3.Dot(velocity, forward);

        // use spring force as the load on the wheels
        float normalForce = Mathf.Max(0f, suspension.Load);

        Vector3 lateralForce = Vector3.zero;

        // get horizontal velcoity and multiply by mass to get a 'force'  (m * g) (need to incorporate my own 'g')
        Vector3 downslope = Vector3.ProjectOnPlane(Physics.gravity, suspension.Normal) * vehicleRigidbody.mass;

        // use the dot to determine how much force is on the "side" axis. should be positive if gravity tries to pull the wheel sideways
        float downslopeSideways = Vector3.Dot(downslope, right);

        // calculate how much sidewasys force is needed to counteract gravity on a slope
        if (Mathf.Abs(sideSpeed) <= almostStoppedSideSpeed)
        {
            float needed = -downslopeSideways;
            float maxHold = gripAtStop * normalForce;
            float appliedHold = Mathf.Clamp(needed * holdReduction, -maxHold, maxHold);

            // apply the holding force (experiencing some bugs where the holding force can pull the vehicle up a slope sideways)
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

        vehicleRigidbody.AddForceAtPosition(lateralForce, suspension.Contact, ForceMode.Force);
        vehicleRigidbody.AddForceAtPosition(rollForce, suspension.Contact, ForceMode.Force);

        Debug.DrawRay(suspension.Contact, lateralForce * 0.001f, Color.green);
        Debug.DrawRay(suspension.Contact, rollForce * 0.001f, Color.red);

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







    /// <summary>
    /// Simple brake that will defo be changed in fututre
    /// </summary>
    public void ApplyBrake(float brakeInput)
    {
        if (suspension == null || vehicleRigidbody == null || !suspension.Grounded || brakeInput <= 0f) return;

        Vector3 contactVelocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        float forwardSpeed   = Vector3.Dot(contactVelocity, forward);
        if (Mathf.Abs(forwardSpeed) < 0.01f) return;

        float desired = Mathf.Clamp01(brakeInput) * maxBrakeForce;
        if (maxBrakeDecelleration > 0f) desired = Mathf.Min(desired, vehicleRigidbody.mass * maxBrakeDecelleration);
        Vector3 force = -Mathf.Sign(forwardSpeed) * forward * desired;
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
        //WheelMesh.localRotation = Quaternion.Euler(WheelMesh.localRotation.x, angle, WheelMesh.localRotation.x);
        WheelMesh.Rotate(Vector3.up, (angle) * Time.fixedDeltaTime);

    }

    public void RotateWheelMesh()
    {
        float wheelCircumference = WheelRadius * Mathf.PI * 2;
        float RotPerSecond = vehicleRigidbody.linearVelocity.magnitude / wheelCircumference;
        WheelMesh.Rotate(Vector3.right, (RotPerSecond * 360) * Time.fixedDeltaTime);
    }
}