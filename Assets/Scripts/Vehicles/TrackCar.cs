using System.Collections.Generic;
using UnityEngine;

public class TrackCar : Vehicle
{
    [Header("Steering & Dynamics")]
    public float maxSteerAngle = 30f;
    public float steerTorque = 200f;

    [Header("Aerodynamic Drag")]
    public bool useDrag = true;
    public float airDensity = 1.225f;
    public float dragCoefficient = 0.32f;
    public float frontalArea = 2.2f;

    public TrackCar(Rigidbody rb, Engine eng, List<Wheel> wheels)
    {
        this.body = rb;
        this.engine = eng;
        this.wheels = wheels;

        foreach (var w in wheels)
        {
            w.SetEngine(engine);
            w.SetRigidbody(body);
        }
    }

    public override void UpdatePhysics(float dt)
    {
        foreach (var w in wheels)
            w.UpdateWheel(dt);

        if (useDrag)
        {
            Vector3 v = body.linearVelocity;
            float speed = v.magnitude;
            if (speed > 0.1f)
            {
                float drag = 0.5f * airDensity * dragCoefficient * frontalArea * speed * speed;
                body.AddForce(-v.normalized * drag, ForceMode.Force);
            }
        }
    }

    public void ApplyInput(float throttle, float steerInput, float brake, float dt)
    {
        float speed = body.linearVelocity.magnitude;
        float torque = engine.GetTorque(throttle, speed, dt);

        foreach (var w in wheels)
        {
            if (w.isFrontWheel)
                w.SetSteerAngle(steerInput * maxSteerAngle);

            w.ApplyDrive(torque);
            w.ApplyBrake(brake);
        }

        body.AddTorque(Vector3.up * steerInput * steerTorque, ForceMode.Force);
    }
}
