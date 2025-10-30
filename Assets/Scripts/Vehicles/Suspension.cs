using UnityEngine;
public class Suspension : MonoBehaviour 
{
    public float restLength = 0.5f;
    public float springStrength = 20000f;
    public float damperStrength = 4500f;
    public LayerMask groundMask;
    public bool isGrounded;
    public Vector3 contactPoint;
    public Vector3 contactNormal = Vector3.up;
    private float lastLength;
    public void UpdateSuspension(float deltaTime, Rigidbody body)
    { 
        isGrounded = false;
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, restLength * 1.5f, groundMask))
        {
            isGrounded = true;
            contactPoint = hit.point;
            contactNormal = hit.normal;

            float currentLength = hit.distance;
            float compression = restLength - currentLength;
            float springForce = compression * springStrength;
            float damperForce = ((lastLength - currentLength) / deltaTime) * damperStrength;

            Vector3 totalForce = transform.up * (springForce + damperForce);

            body.AddForceAtPosition(totalForce, transform.position, ForceMode.Force);
            lastLength = currentLength;
        }
        else
        { 
            lastLength = restLength;
        }
    }
}
