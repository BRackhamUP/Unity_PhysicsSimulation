using UnityEngine;

public class PhysicsManager : MonoBehaviour
{

    /*
       F : Net force vector (N)
       m : mass             (kg)
       a : Acceleration     (m/s^2)
       v : Velocity         (m/s)
       p : Position         (m)
       dt: Delta time       (s)
       Fn: Force Normal     (N)
       0 : Slope angle      (theta)
      
        F = m * a           (Newton second law)
        a = F / m           (acceleration) 
        v = v0 + a * Dt     (velocity over time)
        p = p0 + v * Dt     (position over time)

     */

    [SerializeField]
    private float gravity = -9.81f; 
    [SerializeField]
    Rigidbody rb;

    void FixedUpdate()
    {
       rb.AddForce(new Vector3(0, gravity, 0), ForceMode.Acceleration);
    }
}
