using UnityEngine;
using TMPro;

public class TractionDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tractionText;
    private VehicleController attachedVehicle;

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void AttachToVehicle(VehicleController vc)
    {
        attachedVehicle = vc;
        gameObject.SetActive(true);
    }

    public void Detach()
    {
        attachedVehicle = null;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (attachedVehicle == null) return;
        var vehicleComp = attachedVehicle.currentVehicle;
        if (vehicleComp == null || vehicleComp.wheels == null || vehicleComp.wheels.Count == 0)
        {
            tractionText.text = "--%";
            return;
        }

        float sum = 0f;
        int count = 0;
        foreach (var w in vehicleComp.wheels)
        {
            if (w == null) continue;
            sum += w.GetCurrentTraction();
            count++;
        }
        if (count == 0) { tractionText.text = "--%"; return; }

        float avg = sum / count;
        int percent = Mathf.RoundToInt(Mathf.Clamp01(avg) * 100f);
        tractionText.text = $"{percent}%";
    }
}
