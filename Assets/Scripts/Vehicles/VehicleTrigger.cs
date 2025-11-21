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

    private void OnTriggerEnter(Collider other)
    {
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

