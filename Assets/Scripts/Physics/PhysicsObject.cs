using UnityEngine;

// Script to be applied to all objects utilising the physics manager
public class PhysicsObject : MonoBehaviour
{
    private void FixedUpdate()
    {
        PhysicsManager.ApplyGravity(transform);
    }
}
