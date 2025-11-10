using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles vehicle insances and passes tuning values into them
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleComponent : MonoBehaviour
{
    public enum LogicType { TrackCar, Truck, SuperCar }

    [Header("Engine")]
    public Engine engineData = new Engine();

    [Header("Wheels")]
    public List<Wheel> wheels = new List<Wheel>();

    [Header("Dynamics")]
    public bool useDrag = true;
    public float dragCoefficient = 0.32f;
    public float frontalArea = 2.2f;

    public LogicType vehicleLogicType = LogicType.TrackCar;

    public Vehicle currentVehicleLogic { get; private set; }

    public Transform AttachTransform
    {
        get
        {
            var rb = GetComponent<Rigidbody>();
            return rb != null ? rb.transform : transform;
        }
    }

    private void Awake()
    {
        if (wheels == null || wheels.Count == 0) wheels = new List<Wheel>(GetComponentsInChildren<Wheel>());
        if (engineData != null) engineData.ApplyInspectorUnits();

        var rb = GetComponent<Rigidbody>();

        switch (vehicleLogicType)
        {
            case LogicType.TrackCar:
                var t = new TrackCar(rb, engineData, wheels);
                t.useDrag = useDrag;
                t.dragCoefficient = dragCoefficient;
                t.frontalArea = frontalArea;
                currentVehicleLogic = t;
                break;

            case LogicType.Truck:
                currentVehicleLogic = new Truck(rb, engineData, wheels);
                break;

            case LogicType.SuperCar:
                
                break;
        }
    }

    private void OnValidate()
    {
        if (engineData != null) engineData.ApplyInspectorUnits();
        if (currentVehicleLogic is TrackCar tc)
        {
            tc.useDrag = useDrag;
            tc.dragCoefficient = dragCoefficient;
            tc.frontalArea = frontalArea;
        }
    }

    private void FixedUpdate()
    {
        currentVehicleLogic?.UpdatePhysics(Time.fixedDeltaTime);
    }
}

