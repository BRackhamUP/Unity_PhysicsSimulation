using UnityEngine;

// Script to be applied to all objects that i intend to have adhere to gravity
public class PhysicsObject : MonoBehaviour
{
    private void FixedUpdate()
    {
        PhysicsManager.ApplyGravity(transform);
    }
}
