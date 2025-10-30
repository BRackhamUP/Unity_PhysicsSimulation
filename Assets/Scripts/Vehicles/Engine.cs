using UnityEngine;

public class Engine : MonoBehaviour
{
    public float torque = 800f;
    public float rpm;
    public float maxRPM = 6000f;
    public float rpmDecayRate = 800f;

    public float GetThrottle(float throttle, float deltaTime)
    {

        if (throttle > 0.1f)
            rpm += throttle * 1000f * deltaTime;
        else
            rpm -= rpmDecayRate * deltaTime;

        rpm = Mathf.Clamp(rpm, 0, maxRPM);


        float torqueFactor = Mathf.Lerp(1f, 0.4f, rpm / maxRPM);
        float outputTorque = torque * throttle * Mathf.Clamp01(torqueFactor);

        return outputTorque;
    }
}
