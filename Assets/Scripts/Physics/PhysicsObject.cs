using UnityEngine;

public class PhysicsObject : MonoBehaviour
{
    private void FixedUpdate()
    {
        PhysicsManager.ApplyGravity(transform);
    }
}
