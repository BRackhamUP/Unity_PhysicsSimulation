using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleController : MonoBehaviour
{
    public VehicleComponent currentVehicle; // Assigned when entering vehicle
    public Vehicle vehicleLogic;

    private PlayerControls controls;
    private float throttle;
    private float brake;
    private float steer;

    private void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();
            throttle = input.y;
            steer = input.x;
        };

        controls.Gameplay.Move.canceled += ctx =>
        {
            throttle = 0;
            steer = 0;
        };
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

private void FixedUpdate()
{
    if (vehicleLogic != null)
    {
        vehicleLogic.UpdatePhysics(Time.fixedDeltaTime);

        if (vehicleLogic is TrackCar trackCar)
        {
            trackCar.ApplyThrottle(throttle, Time.fixedDeltaTime, steer);
        }
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
        throttle = 0;
        steer = 0;
    }



        // Later: call methods based on input
        // vehicleLogic.Accelerate(throttle);
        // vehicleLogic.Steer(steer);
}
