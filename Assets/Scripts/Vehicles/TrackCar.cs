using System.Collections.Generic;
using UnityEngine;

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

        foreach (var w in wheels)
            w.UpdateWheel(dt);


        if (useDrag && rb != null)
        {
            Vector3 v = rb.linearVelocity;
            float speed = v.magnitude;
            if (speed > 0.1f)
            {
                float drag = 0.5f * airDensity * dragCoefficient * frontalArea * speed * speed;
                rb.AddForce(-v.normalized * drag, ForceMode.Force);
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
            foreach (var w in wheels) 
                if (w != null && w.isDriven) 
                    drivenCount++;
        }
        drivenCount = Mathf.Max(1, drivenCount);


        float perWheelDrive = totalDriveForce / drivenCount;

        if (wheels != null)
        {
            foreach (var w in wheels)
            {
                if (w == null) continue;

                if (w.isFrontWheel)
                    w.SetSteerAngle(steerInput * maxSteerAngle);

                if (w.isDriven)
                    w.ApplyDriveForce(perWheelDrive);

                w.ApplyBrake(brake);
            }
        }

        rb.AddTorque(Vector3.up * steerInput * steerTorque, ForceMode.Force);
    }
}
