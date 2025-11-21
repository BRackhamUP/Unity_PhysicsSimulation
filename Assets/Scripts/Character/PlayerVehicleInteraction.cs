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

    private bool isInVehicle;
    private VehicleComponent nearbyVehicle;

    private Transform originalFollowTarget;
    private Transform originalLookAtTarget;

    private Vector3 seatOffset = new Vector3(0f, 1f, 0f);
    private Vector3 exitOffset = new Vector3(-3f, 0.5f, 0f);

    private Transform attachTarget;

    private void Awake()
    {
        playerController = GetComponent<PlayerCharacterController>();
        vehicleController = GetComponent<VehicleController>();
        characterCollider = GetComponent<Collider>();
        characterRigidbody = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        InputManager.EnterExitPressed += OnEnterExitPressed;
    }

    private void OnDisable()
    {
        InputManager.EnterExitPressed -= OnEnterExitPressed;
    }
    private void OnEnterExitPressed()
    {
        TryToggleVehicle();
    }

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

        Transform resolved = vehicle.AttachTransform;

        attachTarget = resolved;

        transform.SetParent(attachTarget, true);

        transform.localPosition = seatOffset; 
        transform.localRotation = Quaternion.identity; 

        if (virtualCamera != null)
        {
            originalFollowTarget = virtualCamera.Follow;
            originalLookAtTarget = virtualCamera.LookAt;
            virtualCamera.Follow = attachTarget;
            virtualCamera.LookAt = attachTarget;
        }

        if (playerController != null) playerController.enabled = false;
        SetPlayerVisible(false);

        characterRigidbody.isKinematic = true;
        characterRigidbody.detectCollisions = false;
        characterCollider.enabled = false;

        vehicleController.EnterVehicle(vehicle);

        InputManager.SwitchToVehicle();

        if (speedometer != null)
            speedometer.AttachToVehicle(vehicleController);
    }

    private void ExitVehicle()
    {
        if (!isInVehicle) return;
        isInVehicle = false;

        Vector3 worldExit = Vector3.zero;
        Quaternion worldRot = Quaternion.identity;

        if (attachTarget != null)
        {
            worldExit = attachTarget.TransformPoint(exitOffset);
            worldRot = attachTarget.rotation;
        }
        else if (nearbyVehicle != null)
        {
            worldExit = nearbyVehicle.transform.TransformPoint(exitOffset);
            worldRot = nearbyVehicle.transform.rotation;
        }
        else
        {
            worldExit = transform.position + transform.right * exitOffset.x + Vector3.up * exitOffset.y;
            worldRot = transform.rotation;
        }

        transform.SetParent(null, true);

        transform.position = worldExit + Vector3.up * 0.05f;
        transform.rotation = worldRot;

        SetPlayerVisible(true);

        characterRigidbody.isKinematic = false;
        characterRigidbody.detectCollisions = true;
        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;

        characterCollider.enabled = true;

        if (virtualCamera != null)
        {
            virtualCamera.Follow = originalFollowTarget ?? transform;
            virtualCamera.LookAt = originalLookAtTarget ?? transform;
        }

        if (playerController != null) playerController.enabled = true;

        vehicleController.ExitVehicle();

        InputManager.SwitchToCharacter();

        if (speedometer != null) speedometer.Detach();

        nearbyVehicle = null;
        attachTarget = null;
    }

    private void SetPlayerVisible(bool visible)
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
            r.enabled = visible;
    }
}
