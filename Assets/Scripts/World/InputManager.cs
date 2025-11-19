using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single shared PlayerControls instance and helpers for switching maps.
/// Put one InputManager in the scene root. It creates PlayerControls once and enables Global map.
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour
{
    public static PlayerControls controls { get; private set; }
    static bool initialized = false;

    // Simple event forwarder so listeners don't need to subscribe to InputAction directly.
    public static event System.Action EnterExitPressed;

    void Awake()
    {
        if (initialized)
        {
            Destroy(gameObject);
            return;
        }

        // ensure root before DontDestroyOnLoad
        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        // create the shared PlayerControls instance
        controls = new PlayerControls();

        // always enable the Global map (contains Enter/Exit and other always-on actions).
        controls.Global.Enable();

        // forward the performed callback to an event so code can subscribe decoupled.
        controls.Global.EnterExitVehicle.performed += OnEnterExitPerformed;

        initialized = true;
    }

    private void OnEnterExitPerformed(InputAction.CallbackContext ctx)
    {
        EnterExitPressed?.Invoke();
    }

    void OnDestroy()
    {
        if (initialized && controls != null)
        {
            controls.Global.EnterExitVehicle.performed -= OnEnterExitPerformed;
            controls.Dispose();
            controls = null;
            initialized = false;
        }
    }

    // Convenience helpers other scripts can call (optional)
    public static void SwitchToCharacter()
    {
        if (controls == null) return;
        controls.VehicleControls.Disable();
        controls.CharacterControls.Enable();
    }

    public static void SwitchToVehicle()
    {
        if (controls == null) return;
        controls.CharacterControls.Disable();
        controls.VehicleControls.Enable();
    }
}
