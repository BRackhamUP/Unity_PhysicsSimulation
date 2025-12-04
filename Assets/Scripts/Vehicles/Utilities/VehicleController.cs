using UnityEngine;

/// <summary>
/// recieivng player input and applys to active vehicle logic
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public VehicleComponent currentVehicle;
    public Vehicle vehicleLogic;

    private float rawThrottle;
    private float rawBrake;
    private float rawSteer;

    private float smoothedThrottle = 0f;

    [Header("Throttle Smoothing (tweak)")]
    [Tooltip("How quickly throttle rises (higher = quicker)")]
    [SerializeField] private float throttleAccelResponse = 4f;
    [Tooltip("How quickly throttle falls (higher = faster drop)")]
    [SerializeField] private float throttleDecelResponse = 8f;
    [Tooltip("Small deadzone to ignore tiny trigger input")]
    [SerializeField] private float throttleDeadzone = 0.02f;
    [Tooltip("If true, pressing brake will zero throttle")]
    [SerializeField] private bool brakeCutsThrottle = true;

    private float brake;
    private float steerInput;
    public float currentSpeedMPH;

    private void Update()
    {
        var controls = InputManager.controls;

        if (vehicleLogic is Truck)
        {
            var cargo = currentVehicle.GetComponent<TruckCargo>();
            if (cargo != null)
            {
                if (controls.VehicleControls.TruckPickUp.triggered)
                {
                    cargo.PickUpRock();
                }

                if (controls.VehicleControls.TruckDrop.triggered)
                {
                    cargo.DropRock();
                }
            }
        }

        if (vehicleLogic is SuperCar)
        {
            var activeNitro = currentVehicle.GetComponent<Nitro>();
            if (activeNitro != null)
            {
                if (controls.VehicleControls.Special.triggered)
                {
                    activeNitro.boosting = true;
                    activeNitro.ApplyBoost();
                    Debug.Log("being called at all?");
                }

                if (activeNitro.nitro < 2)
                {
                    activeNitro.boosting = false;
                }

            }
        }
    }

    private void FixedUpdate()
    {
        if (vehicleLogic == null)
            return;

        var controls = InputManager.controls;

        rawThrottle = controls.VehicleControls.Throttle.ReadValue<float>();
        rawSteer = controls.VehicleControls.Steer.ReadValue<float>();
        rawBrake = controls.VehicleControls.Brake.ReadValue<float>();

        if (Mathf.Abs(rawThrottle) < throttleDeadzone) rawThrottle = 0f;

        float targetThrottle = rawThrottle;
        if (brakeCutsThrottle && rawBrake > 0.0001f && targetThrottle > 0f)
        {
            targetThrottle = 0f;
        }

        float deltaTime = Time.fixedDeltaTime;

        float response = (targetThrottle > smoothedThrottle) ? throttleAccelResponse : throttleDecelResponse;

        float alpha = 1f - Mathf.Exp(-response * deltaTime);
        smoothedThrottle = Mathf.Lerp(smoothedThrottle, targetThrottle, alpha);

        steerInput = Mathf.Abs(rawSteer) < 0.12f ? 0f : rawSteer;
        brake = (Mathf.Abs(rawBrake) < 0.02f) ? 0f : rawBrake;

        switch (vehicleLogic)
        {
            case TrackCar trackCar:

                trackCar.ApplyInput(smoothedThrottle, steerInput, brake, deltaTime);

                currentSpeedMPH = trackCar.body.linearVelocity.magnitude * 2.23694f;

                break;

            case Truck truck:

                truck.ApplyInput(smoothedThrottle, steerInput, brake, deltaTime);

                currentSpeedMPH = truck.body.linearVelocity.magnitude * 2.23694f;


                break;

            case SuperCar superCar:

                superCar.ApplyInput(smoothedThrottle, steerInput, brake, deltaTime);

                currentSpeedMPH = superCar.body.linearVelocity.magnitude * 2.23694f;

                break;
        }
    }

    public void EnterVehicle(VehicleComponent newVehicle)
    {
        currentVehicle = newVehicle;
        vehicleLogic = newVehicle.CurrentVehicleLogic;
    }

    public void ExitVehicle()
    {
        currentVehicle = null;
        vehicleLogic = null;
        rawThrottle = rawSteer = smoothedThrottle = 0;
        rawBrake = 1;
    }
}