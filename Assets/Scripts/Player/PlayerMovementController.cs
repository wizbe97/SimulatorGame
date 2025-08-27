using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

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
            // Convert input to world space relative to the player's facing
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDir = transform.TransformDirection(moveDir).normalized;

            float targetSpeed = isSprinting ? stats.SprintSpeed : stats.WalkSpeed;
            Vector3 desired = moveDir * targetSpeed;

            Vector3 velocityChange = desired - velocity;

            // Limit horizontal acceleration per physics step
            float maxDelta = stats.MaxVelocityChange;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxDelta, maxDelta);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxDelta, maxDelta);
            velocityChange.y = 0f;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // Dampen horizontal velocity when no input
            const float dampingFactor = 5f;
            Vector3 horizVel = new Vector3(velocity.x, 0f, velocity.z);
            Vector3 decel = -horizVel * dampingFactor * Time.fixedDeltaTime;

            if (horizVel.sqrMagnitude < 0.01f)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            }
            else
            {
                rb.AddForce(decel, ForceMode.VelocityChange);
            }
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

        // Raycast slightly below the feet on collision layers
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
