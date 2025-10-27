using UnityEngine;


/// <summary>
/// To be attached to the player... when entering a vehicle, i need to place player in vehicle so the camera can maintain its funtionality.
/// </summary>
public class VehicleController : MonoBehaviour
{
    private VehicleComponent vehicleComponent;
    private Vehicle vehicleLogic;

    private void Awake()
    {
        vehicleComponent = GetComponent<VehicleComponent>();
        vehicleLogic = vehicleComponent.vehicleLogic;
    }

    private void Update()
    {
        HandleInput(Time.deltaTime);
    }

    public void HandleInput(float deltaTime)
    {

        //Placeholder input actions for time being...
        float throttle = Input.GetAxis("Verticla");
        float steer = Input.GetAxis("Horizontal");

        if (vehicleLogic != null)
        {
          //  vehicleLogic.Accelerate(throttle);
          //  vehicleLogic.Steer(steer);
        }
    }
}
