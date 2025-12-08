using UnityEngine;

/// <summary>
/// simple nitrous for supercar, need to make UI element for 
/// </summary>
public class Nitro : MonoBehaviour
{
    public Rigidbody vehicleRb;

    [Header("Nitrous Settings")]
    public float maxNitro = 100f;
    public float nitroDrain = 30f;
    public float nitroCharge = 15f;
    public float nitroForce = 200f;

    public bool boosting;

    public float nitro;

    private void Start()
    {
        nitro = maxNitro;
    }

    private void FixedUpdate()
    {
        if (boosting && nitro > 0f)
        {
            ApplyBoost();
        }
        else
        {
            RechargeNitro();
        }        
    }

    // applying the nitro through a constant acceleration until nitro reaches 0
    public void ApplyBoost()
    {
        vehicleRb.AddForce (transform.forward * nitroForce * Time.fixedDeltaTime, ForceMode.Acceleration);

        nitro -= nitroDrain * Time.fixedDeltaTime;

        if (nitro < 0f)
        {
            nitro = 0f;
        }
    }

    // increment the nitro back up after its has been used
    private void RechargeNitro()
    {
        nitro += nitroCharge * Time.fixedDeltaTime;

        if (nitro > maxNitro)
        {
            nitro = maxNitro;
        }
    }

    // to use for displaying to the UI
    public float GetNitroPercentage()
    {
        return nitro / maxNitro;
    }

}
