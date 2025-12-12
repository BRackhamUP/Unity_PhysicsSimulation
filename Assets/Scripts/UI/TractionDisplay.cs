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

    // assiging to the vehicle controller
    public void AttachToVehicle(VehicleController vehicleController)
    {
        attachedVehicle = vehicleController;
        gameObject.SetActive(true);
    }

    // detaching the traction ui from vehicle and display
    public void Detach()
    {
        attachedVehicle = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (attachedVehicle == null) 
            return;

        // get the current vehicle
        var vehicleComp = attachedVehicle.currentVehicle;
        float sum = 0f;
        int count = 0;

        // get the sum of the traction for each wheel 
        foreach (var w in vehicleComp.wheels)
        {
            sum += w.GetCurrentTraction();
            count++;
        }

        // calculate the average traction for all wheels
        float avg = sum / count;

        // round the sum to a precentage to display to UI
        int percent = Mathf.RoundToInt(Mathf.Clamp01(avg) * 100f);
        tractionText.text = $"{percent}%";
    }
}
