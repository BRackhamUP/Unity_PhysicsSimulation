using UnityEngine;

[System.Serializable]
public class Engine
{
    [Header("Engine")]
    public float power = 8000f; // in Newtons
    public float topSpeedMPH = 67f;      // mph
    public float throttleResponse = 8f;

    public float topSpeed = 30f; // m/s
    private float smoothedThrottle = 0f;

    public void ApplyInspectorUnits()
    {
        topSpeed = Mathf.Max(0.01f, topSpeedMPH * 0.44704f); // mph -> m/s
    }

    public float GetDriveForce(float throttle, float speed, float dt)
    {
        float t = Mathf.Clamp(throttle, -1f, 1f);
        smoothedThrottle = Mathf.Lerp(smoothedThrottle, t, Mathf.Clamp01(dt * throttleResponse));

        float total = power * smoothedThrottle;

        if (topSpeed > 0f && smoothedThrottle > 0f)
        {
            float frac = Mathf.Clamp01(speed / topSpeed);

            float taper = 1f - frac * frac;
            total *= Mathf.Clamp01(taper);
        }

        return total;
    }
}
