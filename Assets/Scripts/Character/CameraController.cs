using Unity.Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Zoom Presets")]
    [SerializeField] private float[] zoomLevels = { 5f, 6f, 8f };
    private int currentZoomIndex = 0;

    private PlayerControls controls;
    private CinemachineOrbitalFollow orbital;
    private float zoomLerpSpeed = 5f;
    private float targetZoom;

    private void Awake()
    {
        controls = new PlayerControls();
        controls.CameraControls.ZoomToggle.performed += _ => CycleZoom();

    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start()
    {
        orbital = GetComponent<CinemachineOrbitalFollow>();

        if (orbital == null)
        {
            Debug.LogError("CinemachineOrbitalFollow component not found on camera!");
            return;
        }

        targetZoom = orbital.Radius;
    }

    private void Update()
    {
        if (orbital != null)
        {
            orbital.Radius = Mathf.Lerp(orbital.Radius, targetZoom, Time.deltaTime * zoomLerpSpeed);
        }
    }

    private void CycleZoom()
    {
        currentZoomIndex ++;
        if (currentZoomIndex >= zoomLevels.Length)
        {
            currentZoomIndex = 0;
        }

        targetZoom = zoomLevels[currentZoomIndex];
    }

}
