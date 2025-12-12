using UnityEngine;

/// <summary>
/// simple nitrous for supercar
/// </summary>
public class Nitro : MonoBehaviour
{
    public Rigidbody vehicleRb;

    [Header("Nitrous Settings")]
    public float maxNitro = 100f;
    public float nitroDrain = 30f;
    public float nitroCharge = 15f;
    public float nitroForce = 200f;

    // using a particle system for visual feedback in game
    public ParticleSystem nitroEmitter;

    public bool boosting;
    public float nitro;

    private void Start()
    {
        nitro = maxNitro;
    }

    private void FixedUpdate()
    {
        if (boosting && nitro > 0f)
            ApplyBoost();

        else
            RechargeNitro();

        // particles update based on nitro amount
        UpdateParticles();
    }

    // applying the nitro through a constant acceleration until nitro reaches 0
    public void ApplyBoost()
    {
        // add an acceleration to the vehicle to simulate nitro boost
        vehicleRb.AddForce(transform.forward * nitroForce * Time.fixedDeltaTime, ForceMode.Acceleration);

        // nitro drains over time
        nitro -= nitroDrain * Time.fixedDeltaTime;

        // preventing nitro goign negative
        if (nitro < 0f)
        {
            nitro = 0f;
        }
    }

    // increment the nitro back up after its has been used
    private void RechargeNitro()
    {
        // increase nitro over time
        nitro += nitroCharge * Time.fixedDeltaTime;

        // cap nitro at max to prevent going over limit
        if (nitro > maxNitro)
            nitro = maxNitro;
    }

    private void UpdateParticles()
    {
        // play emitter if boosting and has valid nitro to use
        bool Playing = boosting && nitro > 0f;
        nitroEmitter.Play();

        // stop emitter if no nitro available
        if (!Playing)
            nitroEmitter.Stop();
    }

    // to use for displaying to the UI
    public float GetNitroPercentage()
    {
        return nitro / maxNitro;
    }

}
