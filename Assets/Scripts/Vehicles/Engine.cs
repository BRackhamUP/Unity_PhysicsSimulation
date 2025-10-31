using UnityEngine;

public class Engine : MonoBehaviour
{
    [Header("Engine Setting")]
    public float maxTorque = 800f;
    public float maxRPM = 6000f;
    public float rpmIncreaseRate = 1000f;
    public float rpmDecayRate = 800f;

    public float rpm;

    public float GetThrottle(float throttle, float deltaTime)
    {

        if (throttle > 0.1f)
            rpm += rpmIncreaseRate * throttle * deltaTime;
        else
            rpm -= rpmDecayRate * deltaTime;

        rpm = Mathf.Clamp(rpm, 0, maxRPM);

        // less torque at a high rpm
        float torqueFactor = 1f - (rpm / maxRPM);
        float outputTorque = maxTorque * torqueFactor * throttle;

        return outputTorque;
    }
}
