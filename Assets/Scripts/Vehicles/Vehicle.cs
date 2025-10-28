using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract vehicle base class that owns all parts
/// and defines the basic physical interface.
/// </summary>
public abstract class Vehicle
{
    public float mass;
    public float enginePower;
    public float drag;
    public float maxSpeed;
    public float currentSpeed;

    protected Rigidbody body;
    protected Engine engine;
    protected List<Wheel> wheels;

    public abstract void UpdatePhysics(float deltaTime);
}
