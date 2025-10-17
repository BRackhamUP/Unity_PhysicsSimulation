using UnityEngine;

public class PhysicsManager : MonoBehaviour
{

    [SerializeField]
    private float gravity = -9.81f; 
    [SerializeField]
    Rigidbody rb;

    void FixedUpdate()
    {
       rb.AddForce(new Vector3(0, gravity, 0), ForceMode.Acceleration);
    }
}
