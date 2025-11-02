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

    [HideInInspector] public TrackCar vehicleLogic;
    Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (wheels == null || wheels.Count == 0)
            wheels = new List<Wheel>(GetComponentsInChildren<Wheel>());

        if (engineData != null) engineData.ApplyInspectorUnits();

        vehicleLogic = new TrackCar(rb, engineData, wheels);
        vehicleLogic.useDrag = useDrag;
        vehicleLogic.dragCoefficient = dragCoefficient;
        vehicleLogic.frontalArea = frontalArea;
    }

    private void OnValidate()
    {
        if (engineData != null) engineData.ApplyInspectorUnits();
        if (vehicleLogic != null)
        {
            vehicleLogic.useDrag = useDrag;
            vehicleLogic.dragCoefficient = dragCoefficient;
            vehicleLogic.frontalArea = frontalArea;
        }
    }

    private void FixedUpdate()
    {
        if (vehicleLogic != null) vehicleLogic.UpdatePhysics(Time.fixedDeltaTime);
    }
}
