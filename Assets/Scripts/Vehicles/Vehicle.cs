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

    public abstract void UpdatePhysics(float deltaTime);
}
