using UnityEngine;

/// <summary>
/// trigger collider to notify the player of which vehicle it is near
/// </summary>
public class VehicleTrigger : MonoBehaviour
{
    [SerializeField] private VehicleComponent vehicle;

    private void Awake()
    {
        vehicle = GetComponentInParent<VehicleComponent>();
    }

    // Trigger is a sphere collider located on each vehicle where the player can get in the vehicle
    private void OnTriggerEnter(Collider other)
    {
        // compare player tag to notify vehicle
        if (other.CompareTag("Player"))
        {
            var playerInteraction = other.GetComponent<PlayerVehicleInteraction>();
            playerInteraction.NotifyNearbyVehicle(vehicle);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerInteraction = other.GetComponent<PlayerVehicleInteraction>();
            playerInteraction.NotifyNearbyVehicle(null);
        }
    }
}

