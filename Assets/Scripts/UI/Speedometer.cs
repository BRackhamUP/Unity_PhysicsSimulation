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

    // degrees determined from rotating the needle ui element, from 0 - 200
    private const float MAX_SPEED_ANGLE = -92f;  // needle at max speed
    private const float ZERO_SPEED_ANGLE = 96f;  // needle at zero

    private VehicleController attachedVehicle;
    private float displayedSpeed = 0f;
    private bool isAttached = false;

    // hide ui on awake as game starts with character
    private void Awake()
    {
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isAttached || attachedVehicle == null)
            return;

        // use the current speed MPH variable from vehicle controller to determine the target speed of the needle
        float targetSpeed = attachedVehicle.currentSpeedMPH;
        targetSpeed = Mathf.Clamp(targetSpeed, 0f, speedMax);

        // Lerp the needle movement to provide smooth visual
        displayedSpeed = Mathf.Lerp(displayedSpeed, targetSpeed, 1f - Mathf.Exp(-needleSmoothSpeed * Time.deltaTime));
        float angle = SpeedToAngle(displayedSpeed, speedMax);

        // update the angle of the needle
        needleTransform.localEulerAngles = new Vector3(0f, 0f, angle);
    }

    private float SpeedToAngle(float speedValue, float max)
    {
        // normalise the speed of th eneedle
        float speedNormalised = Mathf.Clamp01(speedValue / max);

        // determine the total angle it will travel 
        float totalAngle = ZERO_SPEED_ANGLE - MAX_SPEED_ANGLE;
        
        // return the speed the needle will travel through the total angle
        return ZERO_SPEED_ANGLE - speedNormalised * totalAngle;
    }

    // attach to the specific vehicle to display and set ui object visible
    public void AttachToVehicle(VehicleController vehicle, float overrideMax = -1f)
    {
        if (vehicle == null) 
            return;

        attachedVehicle = vehicle;
        isAttached = true;
        displayedSpeed = 0f;

        if (overrideMax > 0f) 
            speedMax = overrideMax;

        gameObject.SetActive(true);
    }

    // call detach when leaving vehicle
    public void Detach()
    {
        attachedVehicle = null;
        isAttached = false;
        displayedSpeed = 0f;
        gameObject.SetActive(false);
    }

}
