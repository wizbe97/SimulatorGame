using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerMovementController : MonoBehaviour
{
    [SerializeField] private PlayerStatsSO stats;

    [Header("Optional")]
    [Tooltip("If left empty, will use Camera.main at runtime.")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private PlayerInputHandler inputHandler;

    private Vector2 moveInput;
    private bool isGrounded;
    private bool isSprinting;

    // Jump assist
    private float lastGroundedTime;      // time we were last on ground
    private float lastJumpPressedTime;   // time jump was pressed (buffer)
    private bool jumpQueued;            // we have a jump to try when allowed

    // Advanced jump
    private int airJumpsUsed;            // air jumps since last grounded

    public bool CanMove = true;

    private const float DampingFactor = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputHandler = GetComponent<PlayerInputHandler>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void OnEnable() => SubscribeInput(true);
    private void OnDisable() => SubscribeInput(false);

    private void Update()
    {
        HandleInput();
        FaceCameraDirection();
        CheckGround();
    }

    private void FixedUpdate()
    {
        HandleJump();                 // consume buffer/coyote/air jumps
        if (CanMove) HandleMovement();
        ApplyVariableJumpGravity();   // early-release higher gravity while rising
    }

    // ---------------- Movement ----------------

    private void HandleInput()
    {
        moveInput = inputHandler.MoveInput;

        if (inputHandler.JumpDownThisFrame && stats && stats.EnableJump)
        {
            lastJumpPressedTime = Time.time;
            jumpQueued = true;
        }
    }
    private void HandleMovement()
    {
        if (stats == null) return;

        Vector3 v = rb.velocity;

        if (moveInput != Vector2.zero)
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDir = transform.TransformDirection(moveDir);

            float speed = isSprinting ? stats.SprintSpeed : stats.WalkSpeed;

            // Apex Jump
            bool inApex = !isGrounded && Mathf.Abs(v.y) < stats.ApexDetectionThreshold;
            if (stats.UseApexControl && inApex)
            {
                speed *= stats.ApexModifier;
            }

            Vector3 desiredVelocity = moveDir * speed;

            // Per-axis acceleration toward desired (ignore Y)
            Vector3 delta = desiredVelocity - v;
            float maxDelta = stats.MaxVelocityChange;

            float dx = Mathf.Clamp(delta.x, -maxDelta, maxDelta);
            float dz = Mathf.Clamp(delta.z, -maxDelta, maxDelta);

            rb.AddForce(new Vector3(dx, 0f, dz), ForceMode.VelocityChange);
        }
        else
        {
            // Horizontal damping when no input
            Vector3 horiz = new Vector3(v.x, 0f, v.z);
            if (horiz.sqrMagnitude < 0.01f)
            {
                rb.velocity = new Vector3(0f, v.y, 0f);
            }
            else
            {
                rb.AddForce(-horiz * DampingFactor * Time.fixedDeltaTime, ForceMode.VelocityChange);
            }
        }
    }

    private void HandleJump()
    {
        if (stats == null || !stats.EnableJump)
        {
            jumpQueued = false;
            return;
        }

        bool withinBuffer = (Time.time - lastJumpPressedTime) <= stats.JumpBufferTime;
        bool withinCoyote = (Time.time - lastGroundedTime) <= stats.CoyoteTime;

        if (!(jumpQueued && withinBuffer)) return;

        bool canGroundOrCoyote = isGrounded || withinCoyote;
        bool canAirJump = !canGroundOrCoyote && airJumpsUsed < stats.MaxAirJumps;

        if (canGroundOrCoyote || canAirJump)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            rb.AddForce(Vector3.up * stats.JumpPower, ForceMode.Impulse);
            isGrounded = false;
            jumpQueued = false;

            if (!canGroundOrCoyote)
                airJumpsUsed++;
        }

        if (jumpQueued && !withinBuffer)
            jumpQueued = false;
    }

    private void ApplyVariableJumpGravity()
    {
        if (stats == null || !stats.EnableJump) return;

        bool rising = rb.velocity.y > 0f;
        bool holding = inputHandler.JumpHeld;

        if (rising && !holding)
        {
            float extraMultiplier = Mathf.Max(0f, stats.EndJumpEarlyExtraForceMultiplier - 1f);
            if (extraMultiplier > 0f)
            {
                rb.AddForce(Vector3.down * Physics.gravity.magnitude * extraMultiplier, ForceMode.Acceleration);
            }
        }
    }


    private void SubscribeInput(bool subscribe)
    {
        if (inputHandler == null) return;

        if (subscribe)
        {
            inputHandler.OnSprintStart += HandleSprintStart;
            inputHandler.OnSprintEnd += HandleSprintEnd;
        }
        else
        {
            inputHandler.OnSprintStart -= HandleSprintStart;
            inputHandler.OnSprintEnd -= HandleSprintEnd;
        }
    }

    private void HandleSprintStart() => isSprinting = true;
    private void HandleSprintEnd() => isSprinting = false;

    // ---------------- Helpers ----------------

    private void FaceCameraDirection()
    {
        if (cameraTransform == null) return;

        Vector3 euler = transform.eulerAngles;
        euler.y = cameraTransform.eulerAngles.y;
        transform.eulerAngles = euler;
    }

    private void CheckGround()
    {
        if (stats == null) { isGrounded = false; return; }

        Vector3 origin = new(
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

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
            airJumpsUsed = 0;
        }
    }
}
