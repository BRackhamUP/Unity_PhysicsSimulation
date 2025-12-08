using UnityEngine;
using TMPro;

// displaying the current traction of the vehicle as it increases/decreases speed and passes through different surfaces
public class TractionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tractionText;
    private VehicleController attachedVehicle; 

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void AttachToVehicle(VehicleController vehicleController)
    {
        attachedVehicle = vehicleController;
        gameObject.SetActive(true);
    }

    public void Detach()
    {
        attachedVehicle = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (attachedVehicle == null) 
            return;

        var vehicleComp = attachedVehicle.currentVehicle;
        if (vehicleComp == null || vehicleComp.wheels == null || vehicleComp.wheels.Count == 0)
        {
            tractionText.text = "  %";
            return;
        }

        float sum = 0f;
        int count = 0;

        // get the sum of the traction for each wheel 
        foreach (var w in vehicleComp.wheels)
        {
            if (w == null) 
                continue;

            sum += w.GetCurrentTraction();
            count++;
        }

        if (count == 0) 
        { 
            tractionText.text = "  %"; 
            return; 
        }

        // calculate the average traction for all wheels
        float avg = sum / count;

        // round the sum to a precentage to display to UI
        int percent = Mathf.RoundToInt(Mathf.Clamp01(avg) * 100f);
        tractionText.text = $"{percent}%";
    }
}
