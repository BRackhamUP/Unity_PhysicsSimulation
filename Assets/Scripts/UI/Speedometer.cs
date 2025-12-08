using UnityEngine;

/// <summary>
/// Speedometer to track my vehicles speed, adapted from this 'Code Monkey' tutorial : https://www.youtube.com/watch?v=3xSYkFdQiZ0&t=541s
/// </summary>
[DisallowMultipleComponent]
public class Speedometer : MonoBehaviour
{
    [Header("Needle")]
    [SerializeField] private RectTransform needleTransform;

    [Header("Speed settings")]
    [Tooltip("Max speed (MPH) that corresponds to the far end of the gauge.")]
    [SerializeField] private float speedMax = 200f;

    [Tooltip("How quickly the needle follows the actual speed.")]
    [SerializeField] private float needleSmoothSpeed = 6f;

    private const float MAX_SPEED_ANGLE = -92f;  // needle at max speed
    private const float ZERO_SPEED_ANGLE = 96f;  // needle at zero

    private VehicleController attachedVehicle;
    private float displayedSpeed = 0f; 
    private bool isAttached = false;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isAttached || attachedVehicle == null)
            return;

        float targetSpeed = attachedVehicle.currentSpeedMPH;

        targetSpeed = Mathf.Clamp(targetSpeed, 0f, speedMax);

        displayedSpeed = Mathf.Lerp(displayedSpeed, targetSpeed, 1f - Mathf.Exp(-needleSmoothSpeed * Time.deltaTime));

        float angle = SpeedToAngle(displayedSpeed, speedMax);

        if (needleTransform != null)
            needleTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private float SpeedToAngle(float speedValue, float max)
    {
        float speedNormalized = (max <= 0f) ? 0f : Mathf.Clamp01(speedValue / max);

        float totalAngle = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;
        return ZERO_SPEED_ANGLE - speedNormalized * totalAngle;
    }

    public void AttachToVehicle(VehicleController vehicle, float overrideMax = -1f)
    {
        if (vehicle == null) return;

        attachedVehicle = vehicle;
        isAttached = true;
        displayedSpeed = 0f;

        if (overrideMax > 0f) speedMax = overrideMax;

        gameObject.SetActive(true);
    }

    public void Detach()
    {
        attachedVehicle = null;
        isAttached = false;
        displayedSpeed = 0f;

        gameObject.SetActive(false);
    }

}
