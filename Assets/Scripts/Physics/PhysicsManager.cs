using System.Collections.Generic;
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

    [SerializeField] public Vector3 globalGravity = new Vector3(0, -9.81f, 0);
    [SerializeField] private List<Rigidbody> physicsObjects = new List<Rigidbody>();

    public void RegisterRigidbody(Rigidbody rb)
    {
        if (!physicsObjects.Contains(rb))
        {
            physicsObjects.Add(rb);
        }
    }

    void FixedUpdate()
    {
        foreach (Rigidbody rb in physicsObjects)
        {
            // Gravity
            rb.AddForce(globalGravity, ForceMode.Acceleration);

            // AirDrag
            PhysicsObject po = rb.GetComponent<PhysicsObject>();
            if (po != null) 
            {
                po.ApplyingAirDrag(rb);
            }
        }
    }
}
