using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vehicle subclass that updates wheels, applies drag and distributes drive force amongst wheels
/// </summary>
public class SuperCar : Vehicle
{
    public float maxSteerAngle = 40f;
    public float steerTorque = 150f;

    private Rigidbody rb;

    public SuperCar(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        rb = body;
        this.body = rb;

        this.engine = engine;
        this.wheels = wheels;
    }

    public override void UpdatePhysics(float deltaTime)
    {
        foreach (var wheels in wheels)
            wheels.UpdateWheel(deltaTime);

    }
    public void ApplyInput(float throttle, float steerInput, float brake, float dt)
    {
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