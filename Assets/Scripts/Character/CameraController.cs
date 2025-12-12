using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Adapted from this cinemachine tutorial https://www.youtube.com/watch?v=o7O28SFGWS4&t=523s
/// </summary>
public class CameraController : MonoBehaviour
{
    // default zoom settings for camera, can be adjusted in inspector
    [Header("Zoom Presets")]
    [SerializeField] private float[] zoomLevels = { 5f, 6f, 8f };
    private int currentZoomIndex = 0;

    private CinemachineOrbitalFollow orbital;
    private float zoomLerpSpeed = 5f;
    private float targetZoom;

    private void Awake()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();
        targetZoom = orbital.Radius;
    }

    private void OnEnable()
    {
        // accessing and triggering the controls for zooming 
        InputManager.controls.CameraControls.ZoomToggle.performed += OnZoomTogglePerformed;
        InputManager.controls.CameraControls.Enable();
    }

    private void OnDisable()
    {
        // null check to prevent error occuring on game end
        if (InputManager.controls == null)
            return;

        InputManager.controls.CameraControls.ZoomToggle.performed -= OnZoomTogglePerformed;
        InputManager.controls.CameraControls.Disable();
    }

    private void Update()
    {
        // if updating the orbital radius, have it lerp smoothly between target
        orbital.Radius = Mathf.Lerp(orbital.Radius, targetZoom, Time.deltaTime * zoomLerpSpeed);
    }

    private void OnZoomTogglePerformed(InputAction.CallbackContext _)
    {
        // on zoom increase the zoom index
        currentZoomIndex++;

        // if zoomindex exceeds the length of amount of zoom reset to 0
        if (currentZoomIndex >= zoomLevels.Length) 
            currentZoomIndex = 0;

        // set the target zoom to the current zoom level
        targetZoom = zoomLevels[currentZoomIndex];
    }
}

