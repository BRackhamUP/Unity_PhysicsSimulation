using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract vehicle base class that owns all parts
/// and defines the basic physical interface.
/// </summary>
public abstract class Vehicle
{

    public Rigidbody body;
    public Engine engine;
    public List<Wheel> wheels;

    // shared steering parameters
    public float maxSteerAngle;
    public float steerTorque;

    /// <summary>
    /// this is the overrideable method that will be used in the child vehicle objects to update their moevement
    /// </summary>
    public abstract void UpdatePhysics(float deltaTime);
}
