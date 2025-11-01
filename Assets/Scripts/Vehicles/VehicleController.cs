using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public VehicleComponent currentVehicle;
    public Vehicle vehicleLogic;

    private PlayerControls controls;
    private float throttle;
    private float brake;
    private float steerInput;

    public float currentSpeedMPH;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.JumpBrake.performed += ctx => brake = ctx.ReadValue<float>();
        controls.Gameplay.JumpBrake.canceled += _ => brake = 0f;

        controls.Gameplay.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            throttle = input.y;
            steerInput = input.x;
        };
        controls.Gameplay.Move.canceled += _ =>
        {
            throttle = 0f;
            steerInput = 0f;
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void FixedUpdate()
    {
        if (vehicleLogic == null) return;

        vehicleLogic.UpdatePhysics(Time.fixedDeltaTime);

        if (vehicleLogic is TrackCar car)
        {
            car.ApplyInput(throttle, steerInput, brake, Time.deltaTime);
            currentSpeedMPH = car.body.linearVelocity.magnitude * 2.23694f; // m/s to mph
            Debug.Log($"MPH = {currentSpeedMPH}");
        }
    }

    public void EnterVehicle(VehicleComponent newVehicle)
    {
        currentVehicle = newVehicle;
        vehicleLogic = newVehicle.vehicleLogic;
        Debug.Log($"Entered {vehicleLogic.GetType().Name}");
    }

    public void ExitVehicle()
    {
        Debug.Log($"Exited {vehicleLogic.GetType().Name}");
        currentVehicle = null;
        vehicleLogic = null;
        throttle = steerInput = 0f;
    }
}
