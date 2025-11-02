using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public VehicleComponent currentVehicle;
    public Vehicle vehicleLogic;

    private PlayerControls controls;
    private bool controlsInitialized = false;
    private bool controlsEnabled = false;

    private float throttle;
    private float brake;
    private float steerInput;

    public float currentSpeedMPH;

    private void Awake()
    {
        InitializeControls();
    }

    private void OnEnable()
    {
        InitializeControls();
        if (!controlsEnabled && controls != null)
        {
            controls.Enable();
            controlsEnabled = true;
        }
    }

    private void OnDisable()
    {
        if (controls != null && controlsEnabled)
        {
            controls.Disable();
            controlsEnabled = false;
        }
    }

    private void OnDestroy()
    {
        CleanupControls();
    }

    private void FixedUpdate()
    {
        if (vehicleLogic == null) return;

        if (vehicleLogic is TrackCar car)
        {
            car.ApplyInput(throttle, steerInput, brake, Time.fixedDeltaTime);
            if (car.body != null)
                currentSpeedMPH = car.body.linearVelocity.magnitude * 2.23694f;
           // Debug.Log($"{currentSpeedMPH}");
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

        controls = null;
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
        vehicleLogic = newVehicle.vehicleLogic;
    }

    public void ExitVehicle()
    {
        currentVehicle = null;
        vehicleLogic = null;
        throttle = steerInput = brake = 0f;
    }
}
