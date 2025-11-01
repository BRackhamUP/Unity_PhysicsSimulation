using UnityEngine;

public class Suspension : MonoBehaviour
{
    [Header("Suspension Settings")]
    public float restLength = 0.5f;
    public float springStrength = 20000f;
    public float damperStrength = 4500f;
    public LayerMask groundMask;

    [HideInInspector] public bool grounded;
    [HideInInspector] public Vector3 contact;
    [HideInInspector] public Vector3 normal = Vector3.up;
    [HideInInspector] public float load;

    private float lastLength;

    public void UpdateSuspension(float dt, Rigidbody rb)
    {
        grounded = false;
        load = 0f;

        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, restLength * 1.5f, groundMask))
        {
            grounded = true;
            contact = hit.point;
            normal = hit.normal;

            float len = hit.distance;
            float compression = Mathf.Max(0, restLength - len);

            float springForce = compression * springStrength;
            float damperForce = ((lastLength - len) / Mathf.Max(0.0001f, dt)) * damperStrength;

            float total = springForce + damperForce;
            rb.AddForceAtPosition(transform.up * total, transform.position, ForceMode.Force);

            load = springForce;
            lastLength = len;
        }
        else
        {
            lastLength = restLength;
        }
    }
}
