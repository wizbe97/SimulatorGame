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
        moveInput = inputHandler.MoveInput;
        FaceCameraDirection();
        CheckGround();
    }

    private void FixedUpdate()
    {
        if (CanMove) HandleMovement();
    }

    private void HandleMovement()
    {
        if (stats == null) return;

        Vector3 velocity = rb.velocity;

        if (moveInput != Vector2.zero)
        {
            Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y);
            moveDir = transform.TransformDirection(moveDir);

            float speed = isSprinting ? stats.SprintSpeed : stats.WalkSpeed;
            Vector3 desiredVelocity = moveDir * speed;

            // Change from current to desiredVelocity, clamped per-axis
            Vector3 velocityChange = desiredVelocity - velocity;

            float maxDelta = stats.MaxVelocityChange;
            velocityChange.x = Mathf.Clamp(velocityChange.x, -maxDelta, maxDelta);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -maxDelta, maxDelta);
            velocityChange.y = 0f;

            rb.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            // Horizontal damping when no input
            Vector3 horizVel = new Vector3(velocity.x, 0f, velocity.z);

            if (horizVel.sqrMagnitude < 0.01f)
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            else
            {
                Vector3 decel = DampingFactor * Time.fixedDeltaTime * -horizVel;
                rb.AddForce(decel, ForceMode.VelocityChange);
            }
        }
    }

    private void HandleJump()
    {
        if (stats == null || !stats.EnableJump || !isGrounded) return;

        rb.AddForce(Vector3.up * stats.JumpPower, ForceMode.Impulse);
        isGrounded = false;
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
    }

    private void SubscribeInput(bool subscribe)
    {
        if (inputHandler == null) return;

        if (subscribe)
        {
            inputHandler.OnJump += HandleJump;
            inputHandler.OnSprintStart += HandleSprintStart;
            inputHandler.OnSprintEnd += HandleSprintEnd;
        }
        else
        {
            inputHandler.OnJump -= HandleJump;
            inputHandler.OnSprintStart -= HandleSprintStart;
            inputHandler.OnSprintEnd -= HandleSprintEnd;
        }
    }
}
