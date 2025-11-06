using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleComponent : MonoBehaviour
{
    [Header("Engine (simple)")]
    public Engine engineData = new Engine();

    [Header("Wheels (child objects)")]
    public List<Wheel> wheels = new List<Wheel>();

    [Header("Dynamics (simple)")]
    public bool useDrag = true;
    public float dragCoefficient = 0.32f;
    public float frontalArea = 2.2f;

    public TrackCar vehicleLogicTrackCar;
    public Truck vehicleLogicTruck;
    
    Rigidbody rbTrackCar;
    Rigidbody rbTruck;

    private void Awake()
    {
        rbTrackCar = GetComponent<Rigidbody>();
        rbTruck = GetComponent<Rigidbody>();

        if (wheels == null || wheels.Count == 0)
            wheels = new List<Wheel>(GetComponentsInChildren<Wheel>());

        if (engineData != null) engineData.ApplyInspectorUnits();

        vehicleLogicTrackCar = new TrackCar(rbTrackCar, engineData, wheels);
        vehicleLogicTrackCar.useDrag = useDrag;
        vehicleLogicTrackCar.dragCoefficient = dragCoefficient;
        vehicleLogicTrackCar.frontalArea = frontalArea;

        vehicleLogicTruck = new Truck(rbTruck, engineData, wheels);

    }

    private void OnValidate()
    {
        if (engineData != null) engineData.ApplyInspectorUnits();

        if (vehicleLogicTrackCar != null)
        {
            vehicleLogicTrackCar.useDrag = useDrag;
            vehicleLogicTrackCar.dragCoefficient = dragCoefficient;
            vehicleLogicTrackCar.frontalArea = frontalArea;
        }


    }

    private void FixedUpdate()
    {
        if (vehicleLogicTrackCar != null) vehicleLogicTrackCar.UpdatePhysics(Time.fixedDeltaTime);

        if (vehicleLogicTruck != null) vehicleLogicTruck.UpdatePhysics(Time.fixedDeltaTime);
    }
}
