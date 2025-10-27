using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class PlayerVehicleInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;
    public Transform hidePosition;

    private PlayerCharacterController playerController;
    private Collider characterCollider;
    private Rigidbody characterRigibody;
    private VehicleController vehicleController;
    private PlayerControls controls;

    private bool isInVehicle = false;
    private VehicleComponent nearbyVehicle;

    private void Awake()
    {
        playerController = GetComponent<PlayerCharacterController>();
        vehicleController = GetComponent<VehicleController>();
        characterCollider = GetComponent<Collider>();
        characterRigibody = GetComponent<Rigidbody>();

        controls = new PlayerControls();
        controls.Gameplay.EnterExitVehicle.performed += ctx => TryToggleVehicle();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void NotifyNearbyVehicle(VehicleComponent vehicle)
    {
        nearbyVehicle = vehicle;
        if (vehicle != null)
            Debug.Log($"Player near vehicle: {vehicle.name}");
        else
            Debug.Log("Player left vehicle zone");
    }

    private void TryToggleVehicle()
    {
        if (isInVehicle)
            ExitVehicle();
        else if (nearbyVehicle != null)
            EnterVehicle(nearbyVehicle);
    }

    private void EnterVehicle(VehicleComponent vehicle)
    {
        isInVehicle = true;
        nearbyVehicle = vehicle;

        // Disable player control + visuals
        playerController.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;

        // Disable player physics
        if (characterRigibody != null)
        {
            characterRigibody.isKinematic = true;
            characterRigibody.detectCollisions = false;
        }
        if (characterCollider != null)
            characterCollider.enabled = false;

        // Move & parent to vehicle
        transform.SetParent(vehicle.transform);
        transform.localPosition = Vector3.zero;

        // Give control to vehicle
        vehicleController.EnterVehicle(vehicle);
    }

    private void ExitVehicle()
    {
        isInVehicle = false;
        transform.SetParent(null);

        // Place player next to car
        if (nearbyVehicle != null)
            transform.position = nearbyVehicle.transform.position + nearbyVehicle.transform.right * 2f + Vector3.up * 0.5f;

        // Re-enable visuals
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;

        // Re-enable physics
        if (characterRigibody != null)
        {
            characterRigibody.isKinematic = false;
            characterRigibody.detectCollisions = true;
        }
        if (characterCollider != null)
            characterCollider.enabled = true;

        // Re-enable control
        playerController.enabled = true;
        vehicleController.ExitVehicle();

        nearbyVehicle = null;
    }

}
