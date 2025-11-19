using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles vehicle instances and passes tuning values into them
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleComponent : MonoBehaviour
{
    public enum LogicType { TrackCar, Truck, SuperCar }

    [Header("Engine")]
    public Engine engineData = new Engine();

    [Header("Wheels")]
    public List<Wheel> wheels = new List<Wheel>();

    [Header("Reverse speed clamp")]
    [Tooltip("Maximum allowed reverse speed")]
    public float maxReverseSpeed = 5f;

    [Header("Logic")]
    public LogicType vehicleLogicType = LogicType.TrackCar;

    public Vehicle CurrentVehicleLogic
    {
        get; private set;
    }

    private Rigidbody rb;

    public Transform AttachTransform
    {
        get
        {
            var rigidbody = GetComponent<Rigidbody>();
            return rigidbody != null ? rigidbody.transform : transform;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (wheels == null || wheels.Count == 0) wheels = new List<Wheel>(GetComponentsInChildren<Wheel>());
        if (engineData != null) engineData.ApplyInspectorUnits();

        switch (vehicleLogicType)
        {
            case LogicType.TrackCar:
                var vehicleType = new TrackCar(rb, engineData, wheels);

                CurrentVehicleLogic = vehicleType;
                break;

            case LogicType.Truck:
                CurrentVehicleLogic = new Truck(rb, engineData, wheels);
                break;

            case LogicType.SuperCar:
                CurrentVehicleLogic = new SuperCar(rb, engineData, wheels);
                break;
        }
    }

    private void OnValidate()
    {
        if (engineData != null) engineData.ApplyInspectorUnits();
    }

    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;

        CurrentVehicleLogic?.UpdatePhysics(deltaTime);

        if (rb != null && maxReverseSpeed >= 0f)
            ClampReverseSpeed(rb, maxReverseSpeed);
    }

    private void ClampReverseSpeed(Rigidbody rigidbody, float maxReverse)
    {
        float forwardVel = Vector3.Dot(rigidbody.linearVelocity, transform.forward);

        if (forwardVel < -maxReverse)
        {
            Vector3 lateral = rigidbody.linearVelocity - transform.forward * forwardVel;
            rigidbody.linearVelocity = transform.forward * (-maxReverse) + lateral;
        }
    }
}
