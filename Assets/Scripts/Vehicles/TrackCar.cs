using System.Collections.Generic;
using UnityEngine;
public class TrackCar : Vehicle 
{
    public float steerAngleMax = 30f;

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

    public void ApplyInput(float throttle, float steerInput, float brakeStrength, float deltaTime)
    {
        //Retrieve the torque forom the engine
        float torque = engine.GetThrottle(throttle, deltaTime); 

        foreach (var wheel in wheels)
        {
            wheel.ApplyTorque(torque);
            wheel.ApplyBrake(brakeStrength);
            wheel.SetSteerAngle(steerInput * steerAngleMax);
        }
    }
}
