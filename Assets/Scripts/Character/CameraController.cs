using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

/// <summary>
/// Adapted from this cinemachine tutorial https://www.youtube.com/watch?v=o7O28SFGWS4&t=523s
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Zoom Presets")]
    [SerializeField] private float[] zoomLevels = { 5f, 6f, 8f };
    private int currentZoomIndex = 0;

    private CinemachineOrbitalFollow orbital;
    private float zoomLerpSpeed = 5f;
    private float targetZoom;

    private void Awake()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();
        targetZoom = orbital != null ? orbital.Radius : 5f;
    }

    private void OnEnable()
    {
        InputManager.controls.CameraControls.ZoomToggle.performed += OnZoomTogglePerformed;
        InputManager.controls.CameraControls.Enable();
    }

    private void OnDisable()
    {
        if (InputManager.controls == null) return;
        InputManager.controls.CameraControls.ZoomToggle.performed -= OnZoomTogglePerformed;
        InputManager.controls.CameraControls.Disable();
    }

    private void Update()
    {
        if (orbital != null)
            orbital.Radius = Mathf.Lerp(orbital.Radius, targetZoom, Time.deltaTime * zoomLerpSpeed);
    }

    private void OnZoomTogglePerformed(InputAction.CallbackContext _)
    {
        currentZoomIndex++;
        if (currentZoomIndex >= zoomLevels.Length) currentZoomIndex = 0;
        targetZoom = zoomLevels[currentZoomIndex];
    }
}

