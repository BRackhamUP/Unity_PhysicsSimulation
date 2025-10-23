using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    public float dragCoefficient = 0.1f; //air resistance

    private void FixedUpdate()
    {
        PhysicsManager.ApplyGravity(transform);
    }

    public void ApplyingAirDrag(Rigidbody rb)
    {
        Vector3 drag = -rb.linearVelocity * dragCoefficient;
        rb.AddForce(drag, ForceMode.Force);
    }
}
