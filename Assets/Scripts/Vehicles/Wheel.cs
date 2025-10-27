using UnityEngine;

public class Wheel : MonoBehaviour
{
    public float radius;
    public float traction;
    public float brakeTorque;

    public void ApplyTorque(float torque, float deltaTime)
    {
        //spin the wheel, appply friciton etc...
    }
}
