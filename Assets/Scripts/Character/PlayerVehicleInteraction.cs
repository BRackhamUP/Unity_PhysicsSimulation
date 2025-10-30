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


        playerController.enabled = false;
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = false;


        if (characterRigibody != null)
        {
            characterRigibody.isKinematic = true;
            characterRigibody.detectCollisions = false;
        }
        if (characterCollider != null)
            characterCollider.enabled = false;

        transform.SetParent(vehicle.transform);
        transform.localPosition = new Vector3(0, 1, 0);
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

        vehicleController.EnterVehicle(vehicle);
    }

    private void ExitVehicle()
    {
        isInVehicle = false;
        transform.SetParent(null);

        if (nearbyVehicle != null)
            transform.position = nearbyVehicle.transform.position + nearbyVehicle.transform.right * 2f + Vector3.up * 0.5f;
        transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = true;


        if (characterRigibody != null)
        {
            characterRigibody.isKinematic = false;
            characterRigibody.detectCollisions = true;
        }
        if (characterCollider != null)
            characterCollider.enabled = true;

        playerController.enabled = true;
        vehicleController.ExitVehicle();

        nearbyVehicle = null;
    }

}
