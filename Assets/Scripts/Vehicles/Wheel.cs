using UnityEngine;

/// <summary>
///  wheel is reading the input from the suspension and measuring the wheels motion at the suspensions contact point
///  and then applying forces at that point.
///  sideways force to handle lateral motion
///  rolling resistance to bring the vehicle to a natural stop
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
    [SerializeField][Range(0f, 5f)] private float grip = 0.8f;
    [Tooltip("Grip when vehicle is stopped, to prevent sliding sideways")]
    [SerializeField][Range(0f, 1f)] private float gripAtStop = 0.9f;
    [Tooltip("Resisting sideways motion, higher values will give better cornering")]
    [SerializeField] private float slideResistance = 3000f;
    [Tooltip("A rolling resistance coefficient to slow down vehicle when no throttle is being applied")]
    [SerializeField] private float slowDownResistance = 0.015f;

    [Header("Brakes")]
    [Tooltip("Tick to allow this wheel to be used in braking")]
    [SerializeField] private bool appliesBrake = true;
    [Tooltip("Max brake force this wheel can apply")]
    [SerializeField] private float brakeForce = 4000f;
    [Tooltip("How quickly the wheel brakes")]
    [SerializeField] private float brakeSmooth = 8f;

    private float currentBrake = 0f;
    public float SidewaysSlip { get; private set; }
    public bool IsGrounded { get; private set; }

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

        IsGrounded = suspension.Grounded;

        // if car is not grounded, do not continue to update its physics
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

        SidewaysSlip = Mathf.Abs(sideSpeed);

        // use spring force as the load on the wheels, more load for more grip
        float normalForce = Mathf.Max(0f, suspension.Load);

        // default values when no surface properties are being considered
        float defaultGrip = 0.85f;
        float defaultGripAtStop = 0.99f;
        float defaultSlideMultipler = 1.1f;

        SurfaceProperties surface = suspension.CurrentSurface;

        // surface multipliers for the surface properties
        float gripMult = defaultGrip;
        float gripAtStopMult = defaultGripAtStop;
        float slideMult = defaultSlideMultipler;

        if (surface != null)
        {
            gripMult = surface.gripMultiplier;
            gripAtStopMult = surface.gripAtStopMultiplier;
            slideMult = surface.slideResistanceMultiplier;
        }

        // effective grip used in the calculations
        float effectiveGrip = grip * gripMult;
        float effectiveGripAtStop = gripAtStop * gripAtStopMult;
        float effectiveSlideResistance = slideResistance * slideMult;

        Vector3 lateralForce;

        // if sideways speed is very small, stop vehicle to hold in place
        float lowSideThreshold = 0.4f; // sideways speed cut off point to be considered nearly stopped

        // determine how much gravity pulls sideways along plane
        Vector3 downslope = Vector3.ProjectOnPlane(Physics.gravity, suspension.Normal) * vehicleRigidbody.mass;
        float downslopeSideways = Vector3.Dot(downslope, right);

        if (Mathf.Abs(sideSpeed) <= lowSideThreshold)
        {
            // how much gravity is trying to pull the vehicle
            float needed = -downslopeSideways;

            // max hold force the tire has when nearly stopped
            float maxHold = effectiveGripAtStop * normalForce;

            // apply the amount but clamp to prevent exceeding tire grip
            float appliedHold = Mathf.Clamp(needed * 0.98f, -maxHold, maxHold);

            // apply the lateral force (experiencing some bugs where the holding force can pull the vehicle up a slope sideways dependant on it resistance)
            appliedHold += -sideSpeed * effectiveSlideResistance;
            lateralForce = right * appliedHold;
        }
        else
        {
            // determine a resisting force opposite to sideways movement using sideways velocity
            float resist = -effectiveSlideResistance * sideSpeed;
            float maxResist = effectiveGrip * normalForce;

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

        Debug.DrawRay(suspension.Contact, lateralForce * 0.001f, Color.green);
        Debug.DrawRay(suspension.Contact, rollForce * 0.001f, Color.red);
        Debug.DrawRay(suspension.Contact, suspension.Normal * 0.2f, Color.blue);


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
        if (!appliesBrake) return;
        if (!suspension.Grounded) return;

        // contact forward speed, positive if wheel is rolling forward
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        Vector3 contactVelocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);
        float forwardSpeed = Vector3.Dot(contactVelocity, forward);

        // lerp brake toward 0 to help smoothing
        currentBrake = Mathf.Lerp(currentBrake, 0f, Time.fixedDeltaTime * brakeSmooth);

        // otherwise target a per-wheel brake force and lerp currentBrake toward it
        float target = Mathf.Clamp01(brakeInput) * brakeForce;
        currentBrake = Mathf.Lerp(currentBrake, target, Time.fixedDeltaTime * brakeSmooth);

        // apply braking opposite the direction of motion at the patch
        float sign = Mathf.Sign(forwardSpeed);
        Vector3 force = -sign * forward * currentBrake;
        vehicleRigidbody.AddForceAtPosition(force, suspension.Contact, ForceMode.Force);
    }

    public float GetCurrentTraction()
    {
        // no traction if not grounded
        if (!IsGrounded)
            return 0f;

        // using surface properties when available, else default to 1
        SurfaceProperties surface = suspension.CurrentSurface;
        float surfaceGripMulti = (surface != null) ? surface.gripMultiplier : 1f;
        float surfaceStopGripMulti = (surface != null) ? surface.gripAtStopMultiplier : 1f;

        //determine effctive grip values after applying surface multipliers
        float movingGrip = grip * surfaceGripMulti;
        float staticGrip = gripAtStop * surfaceStopGripMulti;

        // threshold to determine when the wheel is stopped whilst moving sideways
        float staticSlipThreshold = 0.3f;
        float baseline = (SidewaysSlip <= staticSlipThreshold) ? staticGrip : movingGrip;

        // defining how traction fades as sideways slip increases 
        float tractionFadeStartSlip = 0.6f;
        float slipFullPoint = 2.0f;
        float minTractionFraction = 0.25f;


        // when slip is below the fade start, return to baseline
        if (SidewaysSlip <= tractionFadeStartSlip)
            return Mathf.Clamp01(baseline);

        // normalise the slip into 0-1 value
        float slipNormalised = Mathf.Clamp01((SidewaysSlip - tractionFadeStartSlip) / (slipFullPoint - tractionFadeStartSlip));

        // scale traction down smoothly as slip increases
        float tractionScale = Mathf.Lerp(1f, minTractionFraction, slipNormalised);
        float traction = baseline * tractionScale;

        // final traction value clamped between 0-1
        return Mathf.Clamp(traction, 0f, 1f);
    }


    /// <summary>
    ///  simple steering to only apply to front wheelss
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