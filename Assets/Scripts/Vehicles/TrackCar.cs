using System.Collections.Generic;
using UnityEngine;
public class TrackCar : Vehicle
{
    [Header("Steering Settings")]
    public float maxSteerAngle = 30f;
    public float steerTorque = 200f;

    public TrackCar(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        this.body = body;
        this.engine = engine;
        this.wheels = wheels;
    }

    public override void UpdatePhysics(float deltaTime)
    {
        foreach (var wheel in wheels)
            wheel.UpdateWheel(deltaTime);
    }

    public void ApplyThrottle(float throttle, float deltaTime, float steerInput)
    {
        float vehicleSpeed = body.linearVelocity.magnitude;

        // get torque output from engine
        float torque = engine.GetTorqueOutput(throttle, vehicleSpeed, deltaTime);

        // apply that torque to all drive wheels
        foreach (var wheel in wheels)
            wheel.ApplyTorque(torque);

        // steering (visual + simple yaw torque)
        float steerAngle = steerInput * maxSteerAngle;
        foreach (var wheel in wheels)
            wheel.SetSteerAngle(steerAngle);

        // Apply small torque to body to simulate steering turning force
        body.AddTorque(Vector3.up * steerInput * steerTorque, ForceMode.Force);
    }

    public void ApplyBrake(float brakeForce)
    {
        foreach (var wheel in wheels)
            wheel.ApplyBrake(brakeForce);
    }
}