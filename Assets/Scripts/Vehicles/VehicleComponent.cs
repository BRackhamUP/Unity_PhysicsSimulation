using UnityEngine;

/// <summary>
/// place on the vehicles to incporate the abstract vehicle class logic for the individual cars
/// </summary>
public class VehicleComponent : MonoBehaviour
{
    public Vehicle vehicleLogic; // Dependant on which car to drive (Trackcar or Truck so far)

    private void Awake()
    {
        if (vehicleLogic == null)
        {
            if (gameObject.CompareTag("TrackCar"))
            {
                vehicleLogic = new TrackCar();
            }
            else if (gameObject.CompareTag("Truck"))
            {
                vehicleLogic = new Truck();
            }


        }
    }

    private void FixedUpdate()
    {
        //calling the specific vehicle physics update each physics tick
        vehicleLogic?.UpdatePhysics(Time.fixedDeltaTime);
    }

}
