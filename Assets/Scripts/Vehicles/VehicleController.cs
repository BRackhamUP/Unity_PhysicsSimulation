using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// recieivng player input and applys to active vehicle logic
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public VehicleComponent currentVehicle;
    public Vehicle vehicleLogic;

    private PlayerControls controls;
    private bool controlsInitialized;
    private bool controlsEnabled;

    private float throttle;
    private float brake;
    private float steerInput;

    public float currentSpeedMPH;

    private void Awake() => InitializeControls();

    private void OnEnable()
    {
        if (!controlsInitialized) InitializeControls();
        if (!controlsEnabled && controls != null) { controls.Enable(); controlsEnabled = true; }
    }

    private void OnDisable()
    {
        if (controls != null && controlsEnabled) { controls.Disable(); controlsEnabled = false; }
    }

    private void OnDestroy() => CleanupControls();

    private void FixedUpdate()
    {
        if (vehicleLogic == null) return;

        float dt = Time.fixedDeltaTime;

        switch (vehicleLogic)
        {
            case TrackCar trackCar:
                trackCar.ApplyInput(throttle, steerInput, brake, dt);
                if (trackCar.body != null) currentSpeedMPH = trackCar.body.linearVelocity.magnitude * 2.23694f;
                break;

            case Truck truck:
                truck.ApplyInput(throttle, steerInput, brake, dt);
                if (truck.body != null) currentSpeedMPH = truck.body.linearVelocity.magnitude * 2.23694f;
                break;
            case SuperCar superCar:
                superCar.ApplyInput(throttle, steerInput, brake, dt);
                if (superCar.body != null) currentSpeedMPH = superCar.body.linearVelocity.magnitude * 2.23694f;
                break;
        }
    }

    private void InitializeControls()
    {
        if (controlsInitialized) return;

        controls = new PlayerControls();

        controls.Gameplay.JumpBrake.performed += OnBrakePerformed;
        controls.Gameplay.JumpBrake.canceled += OnBrakeCanceled;
        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;

        controlsInitialized = true;
    }

    private void CleanupControls()
    {
        if (!controlsInitialized || controls == null) return;

        controls.Gameplay.JumpBrake.performed -= OnBrakePerformed;
        controls.Gameplay.JumpBrake.canceled -= OnBrakeCanceled;
        controls.Gameplay.Move.performed -= OnMovePerformed;
        controls.Gameplay.Move.canceled -= OnMoveCanceled;

        controls.Dispose();
        controlsInitialized = false;
        controlsEnabled = false;
    }

    private void OnBrakePerformed(InputAction.CallbackContext ctx) => brake = ctx.ReadValue<float>();
    private void OnBrakeCanceled(InputAction.CallbackContext _) => brake = 0f;

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        Vector2 input = ctx.ReadValue<Vector2>();
        throttle = Mathf.Clamp(input.y, -1f, 1f);
        steerInput = input.x;
    }
    private void OnMoveCanceled(InputAction.CallbackContext _) { throttle = 0f; steerInput = 0f; }

    public void EnterVehicle(VehicleComponent newVehicle)
    {
        currentVehicle = newVehicle;
        vehicleLogic = newVehicle?.currentVehicleLogic;
    }

    public void ExitVehicle()
    {
        currentVehicle = null;
        vehicleLogic = null;
        throttle = steerInput = brake = 0f;
    }
}
