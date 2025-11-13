using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// vehicle subclass that updates wheels, applies drag and distributes drive force amongst wheels
/// </summary>
public class TrackCar : Vehicle
{
    public float maxSteerAngle = 30f;
    public float steerTorque = 200f;

    public bool useDrag = true;
    public float airDensity = 1.225f;
    public float dragCoefficient = 0.32f;
    public float frontalArea = 2.2f;

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


        if (useDrag && rb != null)
        {
            Vector3 velocity = rb.linearVelocity;
            float speed = velocity.magnitude;
            if (speed > 0.1f)
            {
                float drag = 0.5f * airDensity * dragCoefficient * frontalArea * speed * speed;
                rb.AddForce(-velocity.normalized * drag, ForceMode.Force);
            }
        }
    }

    public void ApplyInput(float throttle, float steerInput, float brake, float dt)
    {
        if (rb == null || engine == null) return;

        float speed = rb.linearVelocity.magnitude;
        float totalDriveForce = engine.GetDriveForce(throttle, speed, dt);

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
