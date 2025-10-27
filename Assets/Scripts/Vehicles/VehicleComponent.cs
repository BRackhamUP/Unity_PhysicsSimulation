using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class VehicleComponent : MonoBehaviour
{
    public Vehicle vehicleLogic;

    private Rigidbody rb;
    private Engine engine;
    private List<Wheel> wheels = new List<Wheel>();

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        engine = GetComponentInChildren<Engine>();
        wheels.AddRange(GetComponentsInChildren<Wheel>());

        if (vehicleLogic == null)
        {
            if (CompareTag("TrackCar"))
            {
                vehicleLogic = new TrackCar(rb, engine, wheels);
            }
            else if (CompareTag("Truck"))
            {
                vehicleLogic = new Truck(rb, engine, wheels);
            }
        }
    }

    private void FixedUpdate()
    {
        vehicleLogic?.UpdatePhysics(Time.fixedDeltaTime);
    }
}
