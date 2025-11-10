using UnityEngine;

/// <summary>
/// 'get' the contact velocity and compute lateral grip, rolling resistance and apply traction and brakes
/// designed off of coulombs loaw of friciton - https://modern-physics.org/coulombs-law-of-friction/ - " F = \(\mu\)N, where \(\mu\) represents the coefficient of friction "
///  
/// </summary>
public class Wheel : MonoBehaviour
{
    private Suspension suspension;
    private Rigidbody vehicleRigidbody;

    [Header("Wheel Type")]
    [SerializeField] private bool isFrontWheel = false;
    [SerializeField] private bool isDriven = true;

    [Header("Friction and Tyre")]
    // static friciton coefficient, resists sliding at low speeds. coefficient for rubber tire on pavement(ground) - https://www.engineersedge.com/coeffients_of_friction.htm
    [SerializeField] private float muStatic = 0.85f;

    // dynamic friciton coefficient, lower traction at higher speeds
    [SerializeField] private float muDynamic = 0.75f;

    // corner stiffness, convert lateral slip velocity into lateral force, before clamping to muDynamic
    [SerializeField] private float cornerStiffness = 1500f;

    // rolling resistance coefficient
    [SerializeField] private float rollingResistance = 0.015f;

    [Header("Brakes")]
    [SerializeField] private float maxBrakeForce = 15000f; //(N)

    // maximum brake decelleration (M/s`2) used to cap brake force (F = m * a)
    [SerializeField] private float maxBrakeDecelleration = 20f;

    [SerializeField] private float stationaryVehicleThreshold = 0.2f;

    // public read-only accessors for the other systems such as trackCar, truck and superCar 
    public bool IsFrontWheel => isFrontWheel;
    public bool IsDriven => isDriven;
    public Suspension Suspension => suspension;
    public Rigidbody VehicleRigidbody => vehicleRigidbody;

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

        // retrieve the velocity of the vehcile at each tire's contact point.
        Vector3 velocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);

        // project the normalised vector for forward and sideways(right) on to the plane to make the wheel local axis stay flat on the roaf surface
        // originally discovered for third-person char, but made more sense here to incorporate for vehicles traversing hills and over bumps
        // https://discussions.unity.com/t/why-use-projectonplane-in-thirdpersoncharacter-script-of-the-standard-assets-example-project/576001
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;
        Vector3 right = Vector3.ProjectOnPlane(transform.right, suspension.Normal).normalized;

        // determine how much velocity is sideways or forward
        float sideVelocity = Vector3.Dot(velocity, right);
        float forwardVelocity = Vector3.Dot(velocity, forward);

        // use spring force as the load on the wheels
        float normalLoad = suspension.Load;
        normalLoad = Mathf.Max(0f, normalLoad);

        // lateral force
        Vector3 lateralForce = Vector3.zero;

        // determine the gravity force along the wheels lateral axis. F = m * g
        float gravityAlongRight = Vector3.Dot(Physics.gravity * vehicleRigidbody.mass, right);

        // 
        //if wheel is nearly stationary, hold with static friciton
        if (Mathf.Abs(sideVelocity) <= stationaryVehicleThreshold)
        {
            // need to oppose external lateral force
            float needed = -gravityAlongRight;
            float maxStatic = muStatic * normalLoad;
            float applied = Mathf.Clamp(needed * 0.95f, -maxStatic, maxStatic);

            lateralForce = right * applied;
        }
        else
        {
            // wheel slipping sideways simulating drifitng
            float desired = -Mathf.Sign(sideVelocity) * Mathf.Min(muDynamic * normalLoad, Mathf.Abs(sideVelocity) * cornerStiffness);

            lateralForce = right * desired;
        }

        //rolling resistance
        Vector3 rollingForce = Vector3.zero;

        if (Mathf.Abs(forwardVelocity) > stationaryVehicleThreshold)
        {
            // apply a rolling force F = - sign(velocity) * crr * normalLoad
            float rollingResistanceForce = -Mathf.Sign(forwardVelocity) * rollingResistance * normalLoad;

            rollingForce = forward * rollingResistanceForce;

        }
        else
        {
            // if nearly stopped do not apply any force
            rollingForce = Vector3.zero;
        }

        vehicleRigidbody.AddForceAtPosition(lateralForce, suspension.Contact, ForceMode.Force);
        vehicleRigidbody.AddForceAtPosition(rollingForce, suspension.Contact, ForceMode.Force);

        // general debugs
        Debug.DrawRay(suspension.Contact, lateralForce * 0.5f, Color.green);
        Debug.DrawRay(suspension.Contact, rollingForce * 0.5f, Color.red);
    }

    /// <summary>
    /// applying engine drive force to the wheel
    /// </summary>
    public void ApplyDriveForce(float driveForce)
    {
        // if not grounded not driven wheels or not applying drive force, skip
        if (!suspension.Grounded || !isDriven || driveForce == 0)
            return;

        // project normalised vector onto plane
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;

        float tractionLimit = muDynamic * suspension.Load;
        float clamped = Mathf.Clamp(driveForce, -tractionLimit, tractionLimit);

        vehicleRigidbody.AddForceAtPosition(forward * clamped, suspension.Contact, ForceMode.Force);
    }

    public void ApplyBrake(float brakeInput)
    {
        if (!suspension.Grounded || brakeInput <= 0f)
            return;

        Vector3 velocity = vehicleRigidbody.GetPointVelocity(suspension.Contact);
        Vector3 forward = Vector3.ProjectOnPlane(transform.forward, suspension.Normal).normalized;

        // retrieve the dot product of the vectors to determine the 'Sign' = 1 or -1
        float forwardSpeed = Vector3.Dot(velocity, forward);

        // if forward speed is not positive, skip
        if (Mathf.Abs(forwardSpeed) < stationaryVehicleThreshold)
            return;

        // input value * max brake force float
        float brakeForce = Mathf.Clamp01(brakeInput) * maxBrakeForce;

        if (maxBrakeDecelleration > 0f)
        {
            float decellerationLimitForce = vehicleRigidbody.mass * maxBrakeDecelleration;
            brakeForce = Mathf.Min(brakeForce, decellerationLimitForce);
        }

        // forwardSpeed = - (1) * forwardVector direction.normalized (same direction with length of 1) * float  
        Vector3 force = -Mathf.Sign(forwardSpeed) * forward * brakeForce;

        vehicleRigidbody.AddForceAtPosition(force, suspension.Contact, ForceMode.Force);

        Debug.DrawRay(suspension.Contact, force * 0.0002f, Color.blue);
    }

    public void SetSteerAngle(float angleDegrees)
    {
        if (!isFrontWheel) return;
        transform.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
    }
}

