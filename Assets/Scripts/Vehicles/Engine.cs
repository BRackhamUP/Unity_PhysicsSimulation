using UnityEngine;

// https://dev.to/arkilis/systemserializable-in-unity-25hm
[System.Serializable] // using system.Serializable to edit engine values in the inspector even though it is'nt a monbehaviour class

public class Engine
{
    [Header("Engine")]
    public float power = 8000f;          // force of the engine in Newtons
    public float topSpeedMPH = 100f;     // mph
    public float topSpeed = 45f;         // m/s

    // This method is just to have more user friendly variables in the inspecor for tuning the engine
    public void ApplyInspectorUnits()
    {
        topSpeed = Mathf.Max(0.01f, topSpeedMPH * 0.44704f); // (MPH = M/S / 0.44704) - https://www.checkyourmath.com/convert/speed/per_second_hour/m_per_second_miles_per_hour.php
    }

    // Determining the amount of driving force the engine is producing
    public float GetDriveForce(float throttle, float speed, float dt)
    {
        float inputThrottle = Mathf.Clamp(throttle, -1f, 1f); // clamp to prevent crzy throttle values
        float total = power * inputThrottle;
        return total;
    }
}
