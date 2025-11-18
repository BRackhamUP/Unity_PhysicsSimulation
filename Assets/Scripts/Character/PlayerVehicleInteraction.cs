using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// entering/exiting vehicles and switching camera and player states
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class PlayerVehicleInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public float interactRange = 3f;

    [Header("Cinemachine")]
    public CinemachineCamera virtualCamera;

    [Header("UI")]
    [SerializeField] private Speedometer speedometer;

    private PlayerCharacterController playerController;
    private Collider characterCollider;
    private Rigidbody characterRigidbody;
    private VehicleController vehicleController;
    private PlayerControls controls;

    private bool isInVehicle;
    private VehicleComponent nearbyVehicle;

    private Transform originalFollowTarget;
    private Transform originalLookAtTarget;

    private Vector3 seatOffset = new Vector3(0f, 1f, 0f);
    private Vector3 exitOffset = new Vector3(2f, 0.5f, 0f);

    private Transform attachTarget;

    private void Awake()
    {
        playerController = GetComponent<PlayerCharacterController>();
        vehicleController = GetComponent<VehicleController>();
        characterCollider = GetComponent<Collider>();
        characterRigidbody = GetComponent<Rigidbody>();

        controls = new PlayerControls();
        controls.Gameplay.EnterExitVehicle.performed += _ => TryToggleVehicle();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void NotifyNearbyVehicle(VehicleComponent vehicle)
    {
        nearbyVehicle = vehicle;
        Debug.Log(vehicle != null ? $"Player near vehicle: {vehicle.name}" : "Player left vehicle zone");
    }

    private void TryToggleVehicle()
    {
        if (isInVehicle) ExitVehicle();
        else if (nearbyVehicle != null) EnterVehicle(nearbyVehicle);
    }

    private void EnterVehicle(VehicleComponent vehicle)
    {
        if (isInVehicle || vehicle == null) return;

        isInVehicle = true;
        nearbyVehicle = vehicle;

        Transform resolved = null;
        var vc = vehicle as VehicleComponent;
        if (vc != null)
        {
            try { resolved = vc.AttachTransform; } catch { resolved = null; }
        }

        if (resolved == null)
        {
            var rb = vehicle.GetComponent<Rigidbody>();
            resolved = rb != null ? rb.transform : vehicle.transform;
        }

        attachTarget = resolved;


        transform.position = attachTarget.TransformPoint(seatOffset);
        transform.rotation = attachTarget.rotation;

        if (virtualCamera != null)
        {
            originalFollowTarget = virtualCamera.Follow;
            originalLookAtTarget = virtualCamera.LookAt;
            virtualCamera.Follow = attachTarget;
            virtualCamera.LookAt = attachTarget;
        }

        if (playerController != null) playerController.enabled = false;
        SetPlayerVisible(false);

        if (characterRigidbody != null)
        {
            characterRigidbody.isKinematic = true;
            characterRigidbody.detectCollisions = false;
        }
        if (characterCollider != null) characterCollider.enabled = false;

        vehicleController?.EnterVehicle(vehicle);

        if (speedometer != null)
        {
            speedometer.AttachToVehicle(vehicleController);
        }
    }

    private void ExitVehicle()
    {
        if (!isInVehicle) return;
        isInVehicle = false;

        if (attachTarget != null)
        {
            Vector3 worldExit = attachTarget.position + attachTarget.right * exitOffset.x + Vector3.up * exitOffset.y;
            transform.position = worldExit;
            transform.rotation = attachTarget.rotation;
        }
        else if (nearbyVehicle != null)
        {
            transform.position = nearbyVehicle.transform.position + nearbyVehicle.transform.right * exitOffset.x + Vector3.up * exitOffset.y;
            transform.rotation = nearbyVehicle.transform.rotation;
        }

        SetPlayerVisible(true);


        if (characterRigidbody != null)
        {
            characterRigidbody.isKinematic = false;
            characterRigidbody.detectCollisions = true;
        }
        if (characterCollider != null) characterCollider.enabled = true;

        if (virtualCamera != null)
        {
            virtualCamera.Follow = originalFollowTarget ?? transform;
            virtualCamera.LookAt = originalLookAtTarget ?? transform;
        }

        if (playerController != null) playerController.enabled = true;

        vehicleController?.ExitVehicle();

        if (speedometer != null)
        {
            speedometer.Detach();
        }

        nearbyVehicle = null;
        attachTarget = null;
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }
}
