using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vehicle subclass that updates wheels, applies drag and distributes drive force amongst wheels
/// </summary>
public class TrackCar : Vehicle
{
    public float maxSteerAngle = 30f;
    public float steerTorque = 200f;

    private Rigidbody rb;

    public TrackCar(Rigidbody body, Engine engineData, List<Wheel> wheelsList)
    {
        rb = body;
        this.body = rb;

        this.engine = engineData;
        this.wheels = wheelsList;
    }

    public override void UpdatePhysics(float dt)
    {

        foreach (var wheels in wheels)
            wheels.UpdateWheel(dt);

    }

    public void ApplyInput(float throttle, float steerInput, float brake, float dt)
    {
        if (rb == null || engine == null) return;

        float speed = rb.linearVelocity.magnitude;
        float totalDriveForce = engine.GetDriveForce(throttle, speed);

        int drivenCount = 0;
        if (wheels != null)
        {
            foreach (var wheels in wheels) 
                if (wheels != null && wheels.IsDriven) 
                    drivenCount++;
        }
        drivenCount = Mathf.Max(1, drivenCount);


        float perWheelDrive = totalDriveForce / drivenCount;

        if (wheels != null)
        {
            foreach (var wheels in wheels)
            {
                if (wheels == null) continue;

                if (wheels.IsFrontWheel)
                    wheels.SetSteerAngle(steerInput * maxSteerAngle);

                if (wheels.IsDriven)
                    wheels.ApplyDriveForce(perWheelDrive);

                wheels.ApplyBrake(brake);
            }
        }

        rb.AddTorque(Vector3.up * steerInput * steerTorque, ForceMode.Force);
    }
}
