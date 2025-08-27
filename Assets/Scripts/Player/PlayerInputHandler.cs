using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    private PlayerInputActions inputActions;

    public Vector2 MoveInput { get; private set; }
    public Vector2 LookInput { get; private set; }
    public bool IsJumpHeld { get; private set; }

    public delegate void InputActionEvent();
    public event InputActionEvent OnJump;
    public event InputActionEvent OnSprintStart;
    public event InputActionEvent OnSprintEnd;

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        inputActions.Enable();

        inputActions.Movement.Move.performed += ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Movement.Move.canceled += ctx => MoveInput = Vector2.zero;
        inputActions.Movement.Jump.performed += ctx => OnJump?.Invoke();
        inputActions.Movement.Sprint.performed += ctx => OnSprintStart?.Invoke();
        inputActions.Movement.Sprint.canceled += ctx => OnSprintEnd?.Invoke();

        // Look
        inputActions.Camera.Look.performed += ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Camera.Look.canceled += ctx => LookInput = Vector2.zero;
    }

    private void OnDisable()
    {
        inputActions.Movement.Move.performed -= ctx => MoveInput = ctx.ReadValue<Vector2>();
        inputActions.Movement.Move.canceled -= ctx => MoveInput = Vector2.zero;
        inputActions.Movement.Jump.performed -= ctx => OnJump?.Invoke();
        inputActions.Movement.Sprint.performed -= ctx => OnSprintStart?.Invoke();
        inputActions.Movement.Sprint.canceled -= ctx => OnSprintEnd?.Invoke();

        // Look
        inputActions.Camera.Look.performed -= ctx => LookInput = ctx.ReadValue<Vector2>();
        inputActions.Camera.Look.canceled -= ctx => LookInput = Vector2.zero;

        inputActions.Disable();
    }

}
