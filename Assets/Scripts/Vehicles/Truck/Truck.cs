using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vehicle subclass that updates wheels and distributes drive force amongst wheels
/// </summary>
public class Truck : Vehicle
{
    public Truck(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        // assign vehicle references from base class
        this.body = body;
        this.engine = engine;
        this.wheels = wheels;

        // steering parameters defined in vehicle base class
        maxSteerAngle = 30f;
        steerTorque = 200f;
    }

    // update per wheel physics each physics tick
    public override void UpdatePhysics(float deltaTime)
    {
        foreach (var wheel in wheels)
            wheel.UpdateWheel(deltaTime);
    }

    // applying playe rinput to the steering, engine force and braking
    public void ApplyInput(float throttle, float steerInput, float brake, float dt)
    {
        // determine the current vehicle speed for engine force
        float speed = body.linearVelocity.magnitude;
        float totalDriveForce = engine.GetDriveForce(throttle, speed);

        // determine how many wheels are recieving drive force
        int drivenCount = 0;
        if (wheels != null)
        {
            foreach (var wheel in wheels)
                if (wheel != null && wheel.IsDriven)
                    drivenCount++;
        }

        // avoid dividion by ensuring there is atleast 1
        drivenCount = Mathf.Max(1, drivenCount);

        // distribute enigne force across all wheels
        float perWheelDrive = totalDriveForce / drivenCount;

        if (wheels != null)
        {
            // applying the steering, drive force and braking per wheel
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
        // applying additonal torque to assit turnign response
        body.AddTorque(Vector3.up * steerInput * steerTorque, ForceMode.Force);
    }
}