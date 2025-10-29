using System.Collections.Generic;
using UnityEngine;
public class TrackCar : Vehicle 
{
    public float steerAngleMax = 30f;
    public float steerTorque = 200f;
    public TrackCar(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        this.body = body;
        this.engine = engine;
        this.wheels = wheels;
    }
    public override void UpdatePhysics(float deltaTime)
    {
        foreach (var wheel in wheels) wheel.UpdateWheel(deltaTime);
    }
    public void ApplyThrottle(float throttle, float deltaTime, float steerInput = 0f)
    {
        float torque = engine.ApplyThrottle(throttle, deltaTime); 
        foreach (var wheel in wheels) 
            wheel.ApplyTorque(torque);
        float steerDeg = steerInput * steerAngleMax;
        foreach (var wheel in wheels)
            wheel.SetSteerAngle(steerDeg);
        float yawTorque = steerInput * steerTorque;
        body.AddTorque(Vector3.up * yawTorque, ForceMode.Force);
    }
    public void ApplyBrake(float brakeStrength, float deltaTime)
    { 
        foreach (var wheel in wheels)
        {
            wheel.ApplyBrake(brakeStrength); 
        }
    }
}
