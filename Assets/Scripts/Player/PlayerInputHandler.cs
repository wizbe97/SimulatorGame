using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool JumpDownThisFrame { get; private set; }
    public bool JumpHeld { get; private set; }

    public enum LookDevice { Unknown, Mouse, Gamepad, Pointer, Touch }
    public LookDevice LastLookDevice { get; private set; } = LookDevice.Unknown;

    public delegate void InputActionEvent();
    public event InputActionEvent OnSprintStart;
    public event InputActionEvent OnSprintEnd;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null) inputActions = new PlayerInputActions();
        inputActions.Enable();

        // Move
        inputActions.Movement.Move.performed += OnMovePerformed;
        inputActions.Movement.Move.canceled  += OnMoveCanceled;

        // Jump
        inputActions.Movement.Jump.started   += OnJumpStarted;
        inputActions.Movement.Jump.canceled  += OnJumpCanceled;

        // Sprint
        inputActions.Movement.Sprint.performed += OnSprintPerformed;
        inputActions.Movement.Sprint.canceled  += OnSprintCanceled;

        // Look
        inputActions.Camera.Look.performed += OnLookPerformed;
        inputActions.Camera.Look.canceled  += OnLookCanceled;
    }

    private void OnDisable()
    {
        inputActions.Movement.Move.performed -= OnMovePerformed;
        inputActions.Movement.Move.canceled  -= OnMoveCanceled;

        inputActions.Movement.Jump.started   -= OnJumpStarted;
        inputActions.Movement.Jump.canceled  -= OnJumpCanceled;

        inputActions.Movement.Sprint.performed -= OnSprintPerformed;
        inputActions.Movement.Sprint.canceled  -= OnSprintCanceled;

        inputActions.Camera.Look.performed -= OnLookPerformed;
        inputActions.Camera.Look.canceled  -= OnLookCanceled;

        inputActions.Disable();
    }

    private void LateUpdate()
    {
        JumpDownThisFrame = false;
    }

    // -------- Handlers --------

    private void OnMovePerformed(InputAction.CallbackContext ctx) => MoveInput = ctx.ReadValue<Vector2>();
    private void OnMoveCanceled (InputAction.CallbackContext ctx) => MoveInput = Vector2.zero;

    private void OnJumpStarted(InputAction.CallbackContext ctx)
    {
        // This maps to "WasPressedThisFrame"
        JumpDownThisFrame = true;
        JumpHeld = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
        JumpHeld = false;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx) => OnSprintStart?.Invoke();
    private void OnSprintCanceled (InputAction.CallbackContext ctx) => OnSprintEnd?.Invoke();

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        LookInput = ctx.ReadValue<Vector2>();

        var device = ctx.control.device;
        if      (device is Mouse)   LastLookDevice = LookDevice.Mouse;
        else if (device is Gamepad) LastLookDevice = LookDevice.Gamepad;
        else if (device is Pointer) LastLookDevice = LookDevice.Pointer;
        else                        LastLookDevice = LookDevice.Unknown;
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx) => LookInput = Vector2.zero;

    // Sensitivity helper
    public float GetLookSensitivity(CameraSettingsSO settings)
    {
        switch (LastLookDevice)
        {
            case LookDevice.Mouse:
            case LookDevice.Pointer:  return settings.MouseSensitivity;
            case LookDevice.Gamepad:  return settings.ControllerSensitivity;
            default:                  return settings.MouseSensitivity;
        }
    }
}
