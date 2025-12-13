using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// handles vehicle instances and passes tuning values into them
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class VehicleComponent : MonoBehaviour
{
    // define the names of the types of vehicles for runtime logic
    public enum LogicType 
    { 
        TrackCar, 
        Truck, 
        SuperCar 
    }

    [Header("Engine")]
    public Engine engineData = new Engine(); // tune engine in inspector

    [Header("Wheels")]
    public List<Wheel> wheels = new List<Wheel>();

    [Header("Reverse speed clamp")]
    public float maxReverseSpeed = 5f;

    [Header("Logic")]
    public LogicType vehicleLogicType = LogicType.TrackCar; // assign the specific vehicle its logic in the inspector

    // vehicle instance reference
    public Vehicle CurrentVehicleLogic { get; private set; }
    private Rigidbody rb;

    // tranform to attach the camera to, uses vehicle rigidbody or gameobject transform as safety
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
        rb = GetComponent<Rigidbody>();

        // wheels set in inspector, if not take wheel components form children
        if (wheels == null || wheels.Count == 0) 
            wheels = new List<Wheel>(GetComponentsInChildren<Wheel>());
        
        // enigne unit conversion applied in thr inspector, more readable
        if (engineData != null)
            engineData.ApplyInspectorUnits();

        // creating the instance of the vehicle logic 
        switch (vehicleLogicType)
        {
            case LogicType.TrackCar:
                CurrentVehicleLogic = new TrackCar(rb, engineData, wheels);
                break;

            case LogicType.Truck:
                CurrentVehicleLogic = new Truck(rb, engineData, wheels);
                break;

            case LogicType.SuperCar:
                CurrentVehicleLogic = new SuperCar(rb, engineData, wheels);
                break;
        }
    }

    // to ensure the value changes in the inspector 
    private void OnValidate()
    {
        if (engineData != null) 
            engineData.ApplyInspectorUnits();
    }

    // updating the physics of the vehicle and clamping reverse speed
    private void FixedUpdate()
    {
        float deltaTime = Time.fixedDeltaTime;
        CurrentVehicleLogic?.UpdatePhysics(deltaTime);

        // clamp reverse speed
        if (rb != null && maxReverseSpeed >= 0f)
            ClampReverseSpeed(rb, maxReverseSpeed);
    }

    // prevent the vehicle from being able to reverse at the same acceleration speed
    private void ClampReverseSpeed(Rigidbody rb, float maxReverse)
    {
        // forward velocity will be positive in forward direction and negative when reversing
        float forwardVel = Vector3.Dot(rb.linearVelocity, transform.forward);

        if (forwardVel < -maxReverse)
        {
            // only clamp forward/back component and leave lateral motion as is
            Vector3 lateral = rb.linearVelocity - transform.forward * forwardVel;
            rb.linearVelocity = transform.forward * (-maxReverse) + lateral;
        }
    }
}
