using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

    // NEW: camera reference (assign in Inspector or auto-grab Main Camera)
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;

    public bool CanMove = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation; // prevent tipping

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void OnEnable()
    {
        if (inputHandler == null) return;
        inputHandler.OnJump += HandleJump;
        inputHandler.OnSprintStart += HandleSprintStart;
        inputHandler.OnSprintEnd += HandleSprintEnd;
    }

    private void OnDisable()
    {
        if (inputHandler == null) return;
        inputHandler.OnJump -= HandleJump;
        inputHandler.OnSprintStart -= HandleSprintStart;
        inputHandler.OnSprintEnd -= HandleSprintEnd;
    }

    private void Update()
    {
        moveInput = inputHandler != null ? inputHandler.MoveInput : Vector2.zero;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        if (cameraTransform != null)
        {
            var e = transform.eulerAngles;
            e.y = cameraTransform.eulerAngles.y;
            transform.eulerAngles = e;
        }

        CheckGround();
    }

    private void FixedUpdate()
    {
        if (!CanMove) return;
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (stats == null) return;

        Vector3 velocity = rb.velocity;

        if (moveInput != Vector2.zero)
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDir = transform.TransformDirection(moveDir);

            float targetSpeed = isSprinting ? stats.SprintSpeed : stats.WalkSpeed;
            Vector3 desired = moveDir * targetSpeed;

            Vector3 velocityChange = desired - velocity;

            float maxDelta = stats.MaxVelocityChange;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxDelta, maxDelta);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxDelta, maxDelta);
            velocityChange.y = 0f;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            const float dampingFactor = 5f;
            Vector3 horizVel = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 decel = -horizVel * dampingFactor * Time.fixedDeltaTime;

            if (horizVel.sqrMagnitude < 0.01f)
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            else
                rb.AddForce(decel, ForceMode.VelocityChange);
        }
    }

    private void HandleJump()
    {
        if (stats == null) return;
        if (!stats.EnableJump || !isGrounded) return;

        rb.AddForce(Vector3.up * stats.JumpPower, ForceMode.Impulse);
        isGrounded = false;
    }

    private void HandleSprintStart() => isSprinting = true;
    private void HandleSprintEnd() => isSprinting = false;

    private void CheckGround()
    {
        if (stats == null) { isGrounded = false; return; }

        Vector3 origin = new Vector3(
            transform.position.x,
            transform.position.y - (transform.localScale.y * 0.5f),
            transform.position.z
        );

        isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            stats.GroundCheckDistance,
            stats.CollisionLayers,
            QueryTriggerInteraction.Ignore
        );
    }
}
