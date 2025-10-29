using System.Collections.Generic;
using UnityEngine;

public class Truck : Vehicle
{
    public Truck(Rigidbody body, Engine engine, List<Wheel> wheels)
    {
        this.body = body;
        this.engine = engine;
        this.wheels = wheels;
    }

    public override void UpdatePhysics(float deltaTime)
    {

    }
}
