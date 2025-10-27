using UnityEngine;

public class Engine : MonoBehaviour
{
    public float torque = 800f;   // set sensible default in inspector
    public float rpm;
    public float maxRPM = 6000f;
    public float rpmDecayRate = 800f; // RPM/sec decay when no throttle

    public float ApplyThrottle(float throttle, float deltaTime)
    {
        // Increase/decrease rpm
        if (throttle > 0.0001f)
            rpm += throttle * 1000f * deltaTime;
        else
        {
            rpm -= rpmDecayRate * deltaTime;
        }
        rpm = Mathf.Clamp(rpm, 0, maxRPM);

        // simple torque curve
        float torqueFactor = 1f - (rpm / maxRPM);
        torqueFactor = Mathf.Clamp01(torqueFactor);

        float outputTorque = torque * throttle * torqueFactor;
        Debug.Log($"Engine throttle:{throttle:F2} rpm:{rpm:F0} outTorque:{outputTorque:F1}");
        return outputTorque;
    }
}
