using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract vehicle class that will have a 'has a' relationship to all vehicle parts
/// and will be diredctly inherited by types of vehicles
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

    public Vector3 velocity;
    public Vector3 centreOfMass;

    public abstract void UpdatePhysics(float deltaTime);
}
