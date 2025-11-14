using UnityEngine;

// https://dev.to/arkilis/systemserializable-in-unity-25hm
[System.Serializable] // using system.Serializable to edit engine values in the inspector even though it is'nt a monbehaviour class

//convert throttle and speed into a drive force
public class Engine
{
    [Header("Engine")]
    public float power = 8000f;          // force of the engine in Newtons
    public float topSpeedMPH = 100f;     // mph
    [SerializeField]private float topSpeed = 45f;         // m/s

    // This method is just to have more user friendly variables in the inspecor for tuning the engine
    public void ApplyInspectorUnits()
    {
        topSpeed = Mathf.Max(0.01f, topSpeedMPH * 0.44704f); // (MPH = M/S / 0.44704) - https://www.checkyourmath.com/convert/speed/per_second_hour/m_per_second_miles_per_hour.php
    }

    // Determining the amount of driving force the engine is producing
    public float GetDriveForce(float throttle, float speed)
    {
        float inputThrottle = Mathf.Clamp(throttle, -1f, 1f); // clamp to prevent crzy throttle values
        float total = power * inputThrottle;

        // Reduce force as approach topspeed, but are still throttling
        if (topSpeed > 0f && inputThrottle > 0)
        {
            // calculate the soeed fraction between 0 and 1,  to determine the taper
            float speedFraction = Mathf.Clamp01(speed / topSpeed);

            // minus the fraction from 1 to give the inverted value 
            float taper = 1f - speedFraction;

            // by multiplying the total by the taper it will gradually reduce engine power when vehicle reaches its max speed
            total = total * taper;
        }
        return total;
    }
}
