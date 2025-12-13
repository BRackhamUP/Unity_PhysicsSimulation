using UnityEngine;

/// <summary>
/// using a raycast to detect the ground layer and compute spring and damper forces and apply vertical forces to rigidbody
/// </summary>
public class Suspension : MonoBehaviour
{
    [Header("Suspension")]
    [SerializeField] private float restLength = 0.5f;
    [SerializeField] private float maxLength = 0.5f;
    [SerializeField] private float springStrength = 20000f;
    [SerializeField] private float damperStrength = 4500f;
    [SerializeField] private LayerMask groundMask;

    // read only variables
    public bool Grounded { get; private set; }
    public Vector3 Contact { get; private set; }
    public Vector3 Normal { get; private set; } = Vector3.up;

    // the current vertical load on the wheel
    public float Load { get; private set; }

    // last length of the raycast to determine the damper velocity
    private float lastLength;

    public SurfaceProperties CurrentSurface { get; private set; }

    /// <summary>
    /// updating the suspension for 'this' wheel. happens for all instances of wheel on vehicle
    /// </summary>
    public void UpdateSuspension(float deltaTime, Rigidbody rigidbody)
    {
        Grounded = false;
        Load = 0f;

        //the length of the ray is bigger then the restlength to detect the ground beyond the restlength of suspension
        float rayLength = maxLength;
        Debug.DrawRay(transform.position, -transform.up * rayLength, Color.red, 0.1f);

        // fire the raycast from the wheels transform position downwards, get info at the raylength and detect groundlayer
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, rayLength, groundMask))
        {
            Grounded = true;
            Contact = hit.point;
            Normal = hit.normal;

            CurrentSurface = hit.collider.GetComponentInParent<SurfaceProperties>();

            // distance from the transform to the contact point. current length of the suspension
            float length = hit.distance;

            // Hookes Law suspension - https://www.engineeringtoolbox.com/hookes-law-force-spring-constant-d_1853.html
            // F = KS
            // calculate the compression of the suspension which is the displacement (S)
            // springStrength is the constant (K)
            // springForce is the result (F)
            float compression = Mathf.Max(0f, restLength - length);
            float springForce = compression * springStrength;

            // damping force: F = cx - (Slide 12) https://canvas.anglia.ac.uk/courses/48081/pages/csfg-lecture-6-session-a-suspension?module_item_id=2876804
            // compression velocity is the change in length over time
            float velocity = (lastLength - length) / Mathf.Max(0.0001f, deltaTime);
            float damperForce = velocity * damperStrength;

            // combining spring and damper forces
            float total = springForce + damperForce;

            // 'transform.up' accounts for the '-' in hookes original formula as the upwards transform is the negative of the compression direction
            rigidbody.AddForceAtPosition(transform.up * total, hit.point, ForceMode.Force);

            // store spring force for traction calc and lastlength for next frame
            Load = springForce;
            lastLength = length;
        }
        else
        {
            // no ground contact returns the suspension length to restlength
            lastLength = restLength;

            CurrentSurface = null;
        }
    }
}

