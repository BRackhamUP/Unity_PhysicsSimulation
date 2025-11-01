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

[System.Serializable]
public class Engine : MonoBehaviour
{
    [Header("Engine Settings")]
    public float maxTorque = 800f;
    public float maxRPM = 6000f;
    public float idleRPM = 800f;
    public float rpmResponse = 4f;

    [Header("Transmission")]
    public float gearRatio = 2f;
    public float finalDrive = 1.8f;
    public float wheelRadius = 0.40f;
    [Range(0.5f, 1f)] public float efficiency = 0.9f;

    public float rpm;

    public float GetTorque(float throttle, float speed, float dt)
    {
        float wheelRPM = (speed / (2 * Mathf.PI * wheelRadius)) * 60f;
        float targetRPM = Mathf.Max(idleRPM, wheelRPM * gearRatio * finalDrive);

        rpm = Mathf.Lerp(rpm, targetRPM, dt * rpmResponse);
        rpm = Mathf.Clamp(rpm, idleRPM, maxRPM);

        float curve = Mathf.Sin((rpm / maxRPM) * Mathf.PI);
        return maxTorque * curve * throttle * efficiency * finalDrive;
    }
}


