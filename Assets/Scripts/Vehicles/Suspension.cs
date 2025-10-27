using UnityEngine;

public class Suspension : MonoBehaviour
{
    public float restLength = 0.5f;
    public float springStrength = 20000f;
    public float damperStrength = 4500f;
    public LayerMask groundMask;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public Vector3 contactPoint;
    [HideInInspector] public Vector3 contactNormal;
    private float lastLength;

    public float UpdateSuspension(float deltaTime, Rigidbody body)
    {
        isGrounded = false;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, restLength * 1.5f, groundMask))
        {
            isGrounded = true;
            contactPoint = hit.point;
            contactNormal = hit.normal;

            float currentLength = hit.distance;
            float compression = Mathf.Clamp01((restLength - currentLength) / restLength);

            float springForce = compression * springStrength;
            float damperForce = ((lastLength - currentLength) / deltaTime) * damperStrength;

            Vector3 totalForce = transform.up * (springForce + damperForce);
            body.AddForceAtPosition(totalForce, transform.position, ForceMode.Force);

            lastLength = currentLength;
            return compression;
        }

        lastLength = restLength;
        return 0f;
    }
}
