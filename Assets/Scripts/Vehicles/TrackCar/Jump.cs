using UnityEngine;
using UnityEngine.InputSystem;

public class Jump : MonoBehaviour
{
    public Rigidbody vehicleRb;

    [Header("Nitrous Settings")]
    public float maxJumpForce = 100f;
    public float downTime = 10f;


    private void FixedUpdate()
    {

    }

    public void ApplyJump()
    {
        vehicleRb.AddForce(transform.forward * maxJumpForce * Time.fixedDeltaTime, ForceMode.Impulse);


    }

}
