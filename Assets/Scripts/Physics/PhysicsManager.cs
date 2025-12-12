using UnityEngine;

public static class PhysicsManager
{
    
    /*      relevent units
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

    // static method to apply gravity to obejcts without using the rigidbody built in gravity
    public static void ApplyGravity(Transform gameObject)
    {
        Rigidbody objectRigidbody = gameObject.GetComponent<Rigidbody>();
         
         objectRigidbody.AddForce(new Vector3(0, -9.81f, 0), ForceMode.Acceleration);       
    }
}
