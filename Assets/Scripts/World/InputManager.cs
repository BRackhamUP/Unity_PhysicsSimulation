using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single shared PlayerControls instance and helpers for switching controls
/// NEED TO REFERENCE
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour
{
    public static PlayerControls controls { get; private set; }
    static bool initialized = false;

    public static event System.Action EnterExitPressed;

    void Awake()
    {
        if (initialized)
        {
            Destroy(gameObject);
            return;
        }

        if (transform.parent != null) transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        controls = new PlayerControls();

        controls.Global.Enable();

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
