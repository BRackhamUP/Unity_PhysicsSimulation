using UnityEngine;

public class Suspension : MonoBehaviour
{
    public float stiffness;
    public float damping;
    public float restLength;
    public float compression;

    public void UpdateSuspension(float deltaTime, float wheelLoad)
    {
        //simulate spring and damping forces etc...
    }
}
