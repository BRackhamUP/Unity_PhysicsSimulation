using System.Collections.Generic;
using UnityEngine;

public class TrackCar : Vehicle
{
    public float downforce;
    public float steerAngleMax = 30f;
    public float steerTorque = 200f; // torque applied to yaw the body

    public TrackCar(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        this.body = body;   
        this.engine = engine;
        this.wheels = wheels;
    }

    // call every fixed update
    public override void UpdatePhysics(float deltaTime)
    {
        foreach (var wheel in wheels)
        {
            wheel.suspension.UpdateSuspension(deltaTime, body);
        }

        if (Input.GetKey(KeyCode.W))
        {
            foreach (var wheel in wheels)
            {
                wheel.ApplyTorque(2000f, deltaTime);
            }
        }
    }


    public void ApplyThrottle(float throttle, float deltaTime, float steerInput = 0f)
    {
        float torque = engine.ApplyThrottle(throttle, deltaTime);
        Debug.Log($"Torque from engine: {torque}, Rigidbody mass: {body.mass}");

        // apply drive per wheel (no deltaTime multiplication)
        foreach (var wheel in wheels)
            if (Mathf.Abs(throttle) > 0.01f)
                wheel.ApplyTorque(torque, deltaTime);

        // steering: rotate front wheels visually and apply yaw torque to body
        float steerDeg = steerInput * steerAngleMax;
        foreach (var wheel in wheels)
            wheel.SetSteerAngle(steerDeg);

        // apply yaw torque proportional to steer * speed
        float speedFactor = Mathf.Clamp01(body.linearVelocity.magnitude / 30f);
        float yawTorque = steerInput * steerTorque * (1f + speedFactor);
        body.AddTorque(body.transform.up * yawTorque, ForceMode.Force);
    }
}
