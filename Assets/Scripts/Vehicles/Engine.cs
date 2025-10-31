using UnityEngine;
/*
 * Engine power.
 * required power form an engine to keep a car at constant speed can be calculated as:
 *          P = FT V / n
 *     P = enigne power in (W)
 *    FT = total forces acting on the car - rolling resistance & aerodynamic drag
 *     v = velocity of the car
 *     n = overall efficiency in the transmission, normally ranging 0.85(low gear) - 0.9(direct dirve)
 *     
 *         engine power to go 90 km/h with 0.85 efficeincy
 *     P = (90 km/h) (1000 m/km) (1/3600 h/s) / 0.85
 *       = 19118 W
 *       = 19 kW
 *       
 *       https://www.engineeringtoolbox.com/cars-power-torque-d_1784.html
 */
public class Engine : MonoBehaviour
{
    [Header("Engine Settings")]
    public float maxTorque = 800f;
    public float maxRPM = 6000f;
    public float idleRPM = 800f;
    public float rpmResponseRate = 5f;

    [Header("Transmission Settings")]
    public float gearRatio = 3.5f;
    public float finalDriveRatio = 3.2f;
    public float wheelRadius = 0.35f;
    [Range(0.5f, 1f)] public float transmissionEfficiency = 0.9f;

    public float rpm;
    public float currentTorque;

    public float GetTorqueOutput(float throttle, float vehicleSpeed, float deltaTime)
    {
        float wheelRPM = (vehicleSpeed / wheelRadius) * (60f / (2f * Mathf.PI));
        float targetRPM = Mathf.Max(idleRPM, wheelRPM * gearRatio * finalDriveRatio);
        rpm = Mathf.Lerp(rpm, targetRPM, deltaTime * rpmResponseRate);
        rpm = Mathf.Clamp(rpm, idleRPM, maxRPM);

        float rpmPercent = rpm / maxRPM;
        float torqueCurve = Mathf.Sin(rpmPercent * Mathf.PI); 

        currentTorque = maxTorque * torqueCurve * throttle * transmissionEfficiency;
        return currentTorque * finalDriveRatio;
    }
}
