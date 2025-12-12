using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Single shared PlayerControls instance and helpers for switching controls
/// </summary>
[DefaultExecutionOrder(-100)]
public class InputManager : MonoBehaviour
{
    // access for play controls for reading input
    public static PlayerControls controls { get; private set; }
    static bool initialized = false;

    // event for entering and exiting input
    public static event System.Action EnterExitPressed;

    void Awake()
    {
        // ensure there is only one instance of inputmanger
        if (initialized)
        {
            Destroy(gameObject);
            return;
        }
        controls = new PlayerControls();

        // enable the gloal action map so its active regardless of player/vehicle state
        controls.Global.Enable();
        controls.Global.EnterExitVehicle.performed += OnEnterExitPerformed;

        initialized = true;
    }

    //callback the enter/exit input action to call the event
    private void OnEnterExitPerformed(InputAction.CallbackContext ctx)
    {
        // safety '?' to check for null references
        EnterExitPressed?.Invoke();
    }

    // switching controls to character controls when exiting vehicle
    public static void SwitchToCharacter()
    {
        if (controls == null) 
            return;

        controls.VehicleControls.Disable();
        controls.CharacterControls.Enable();
    }

    // switching controls to vehicle controls when getting in vehicle
    public static void SwitchToVehicle()
    {
        if (controls == null) 
            return;

        controls.CharacterControls.Disable();
        controls.VehicleControls.Enable();
    }
}
