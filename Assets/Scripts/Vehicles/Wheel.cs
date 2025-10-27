using UnityEngine;

public class Wheel : MonoBehaviour
{
    public float traction = 1.0f;
    public float brakeTorque = 300f;
    public Suspension suspension;
    public Engine engine;
    [Tooltip("If true the wheel visually steers (front wheels).")]
    public bool isFrontWheel = false;
    [Tooltip("Local steering pivot (optional).")]
    public Transform steeringPivot;

    public void Awake()
    {
        if (suspension == null) suspension = GetComponent<Suspension>();
        if (engine == null) engine = GetComponentInParent<Engine>(); // fallback
    }

    public void UpdateWheel(float deltaTime, Rigidbody rb)
    {
        if (rb == null || suspension == null) return;
        suspension.UpdateSuspension(deltaTime, rb);

        // wheel spin visual (optional)
        if (rb != null && rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            transform.Rotate(Vector3.right, rb.linearVelocity.magnitude * deltaTime * 100f, Space.Self);
        }
    }

    public void ApplyTorque(float torque, float deltaTime)
    {
        if (suspension == null || !suspension.isGrounded)
            return;

        Rigidbody rb = GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        // Tangential drive direction along the ground
        Vector3 driveDir = Vector3.Cross(suspension.contactNormal, -transform.right).normalized;

        Vector3 driveForce = driveDir * torque;
        rb.AddForceAtPosition(driveForce, suspension.contactPoint, ForceMode.Force);

        Debug.DrawRay(suspension.contactPoint, driveForce.normalized * 3f, Color.blue);
    }


    public void UpdateWheelVisual(float rpm, float steerAngle)
    {
        // Spin the wheel based on RPM
        transform.Rotate(Vector3.right, rpm * Time.deltaTime, Space.Self);

        // Steer only front wheels
        if (steerAngle != 0)
            transform.localRotation = Quaternion.Euler(0f, steerAngle, 0f);
    }


    public void SetSteerAngle(float angleDegrees)
    {
        if (!isFrontWheel) return;
        if (steeringPivot != null)
            steeringPivot.localRotation = Quaternion.Euler(0f, angleDegrees, 0f);
        else
            transform.localRotation = Quaternion.Euler(0f, angleDegrees, transform.localEulerAngles.z);
    }
}
