using Unity.Cinemachine;
using UnityEngine;

/// <summary>
/// entering/exiting vehicles and switching camera and player states
/// </summary>
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class PlayerVehicleInteraction : MonoBehaviour
{
    [Header("Cinemachine")]
    public CinemachineCamera virtualCamera;

    [Header("UI")]
    [SerializeField] private Speedometer speedometer;
    [SerializeField] private TractionDisplay tractionDisplay;

    private PlayerCharacterController playerController;
    private Collider characterCollider;
    private Rigidbody characterRigidbody;
    private VehicleController vehicleController;

    private bool isInVehicle;
    private VehicleComponent nearbyVehicle;
    private Transform attachTarget;

    private Transform originalFollowTarget;
    private Transform originalLookAtTarget;

    private Vector3 seatOffset = new Vector3(0f, 1f, 0f);
    private Vector3 exitOffset = new Vector3(-3f, 0.5f, 0f);

    // get the component references 
    private void Awake()
    {
        playerController = GetComponent<PlayerCharacterController>();
        vehicleController = GetComponent<VehicleController>();
        characterCollider = GetComponent<Collider>();
        characterRigidbody = GetComponent<Rigidbody>();
    }

    // hooking up my inputs to the method 
    private void OnEnable()
    {
        InputManager.EnterExitPressed += OnEnterExitPressed;
    }

    private void OnDisable()
    {
        InputManager.EnterExitPressed -= OnEnterExitPressed;
    }

    // input will try to toggle enter/exit logic 
    private void OnEnterExitPressed()
    {
        TryToggleVehicle();
    }

    // Called by vehicles trigger when player is near 
    public void NotifyNearbyVehicle(VehicleComponent vehicle)
    {
        nearbyVehicle = vehicle;
    }

    // simple toggle logic for entering and exiting vehicle
    private void TryToggleVehicle()
    {
        if (isInVehicle)
            ExitVehicle();

        else if (nearbyVehicle != null)
            EnterVehicle(nearbyVehicle);
    }

    // using vehicle component, put player into the vehicle
    private void EnterVehicle(VehicleComponent vehicle)
    {
        if (isInVehicle || vehicle == null) 
            return;

        isInVehicle = true;
        nearbyVehicle = vehicle;
        attachTarget = vehicle.AttachTransform;

        // parent the player to the vehicle 
        transform.SetParent(attachTarget, true);
        transform.localPosition = seatOffset;
        transform.localRotation = Quaternion.identity;

        // switching the camera to follow and look at new target
        originalFollowTarget = virtualCamera.Follow;
        originalLookAtTarget = virtualCamera.LookAt;
        virtualCamera.Follow = attachTarget;
        virtualCamera.LookAt = attachTarget;

        //disbale player controller and visibility
        playerController.enabled = false;
        SetPlayerVisible(false);
        characterRigidbody.isKinematic = true;
        characterRigidbody.detectCollisions = false;
        characterCollider.enabled = false;

        // set the vehicle controller and call to switch inputs
        vehicleController.EnterVehicle(vehicle);
        InputManager.SwitchToVehicle();

        // attach vehicle UI
        speedometer.AttachToVehicle(vehicleController);
        tractionDisplay.AttachToVehicle(vehicleController);

        // start vehicle idle audio
        AudioManager.Instance?.Play("VehicleIdle", loop: true);
    }

    // removing player from vehicle 
    private void ExitVehicle()
    {
        if (!isInVehicle) return;
        isInVehicle = false;

        // find world exit position for player 
        // Can experience a bug where the player is thrown across the map ? not sure why
        Vector3 worldExit;
        Quaternion worldRot;
        worldExit = transform.position + transform.right * exitOffset.x + Vector3.up * exitOffset.y;
        worldRot = transform.rotation;

        // remove parent and place above ground to prevent clipping throught he floor
        transform.SetParent(null, true);
        transform.position = worldExit + Vector3.up * 0.05f;
        transform.rotation = worldRot;

        // make visible again and restore parameters
        SetPlayerVisible(true);
        characterRigidbody.isKinematic = false;
        characterRigidbody.detectCollisions = true;
        characterRigidbody.linearVelocity = Vector3.zero;
        characterRigidbody.angularVelocity = Vector3.zero;
        characterCollider.enabled = true;

        // replace camera back to player
        virtualCamera.Follow = originalFollowTarget ?? transform;
        virtualCamera.LookAt = originalLookAtTarget ?? transform;

        // switch controller and inputs back to character 
        playerController.enabled = true;
        vehicleController.ExitVehicle();
        InputManager.SwitchToCharacter();

        // remove UI
        speedometer.Detach();
        tractionDisplay.Detach();

        // clear the vehicle and camera references and stop audio
        nearbyVehicle = null;
        attachTarget = null;
        AudioManager.Instance?.Stop("VehicleIdle");
        AudioManager.Instance?.Stop("VehicleThrottle");
    }

    // simple toggle for player visibility
    private void SetPlayerVisible(bool visible)
    {
        foreach (var renderer in GetComponentsInChildren<Renderer>())
            renderer.enabled = visible;
    }
}
