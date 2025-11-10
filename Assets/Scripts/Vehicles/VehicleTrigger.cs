using UnityEngine;

/// <summary>
/// trigger collider to notify the player of which vehicle it is near
/// </summary>
public class VehicleTrigger : MonoBehaviour
{
    [SerializeField] private VehicleComponent vehicle; 

    private void Awake()
    {
        if (vehicle == null)
            vehicle = GetComponentInParent<VehicleComponent>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerInteraction = other.GetComponent<PlayerVehicleInteraction>();
            if (playerInteraction != null)
            {
                Debug.Log($"Player entered trigger for {vehicle.name}");
                playerInteraction.NotifyNearbyVehicle(vehicle);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerInteraction = other.GetComponent<PlayerVehicleInteraction>();
            if (playerInteraction != null)
            {
                Debug.Log($"Player exited trigger for {vehicle.name}");
                playerInteraction.NotifyNearbyVehicle(null);
            }
        }
    }
}

