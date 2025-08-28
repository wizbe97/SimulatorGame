using UnityEngine;

[CreateAssetMenu(menuName = "Game/Player Stats")]
public class PlayerStatsSO : ScriptableObject
{
    [Header("Setup")]
    [Tooltip("Layer the player is on (used to exclude self in checks)")]
    public LayerMask PlayerLayer;

    [Tooltip("Layers considered solid for collisions, ground, walls, etc.")]
    public LayerMask CollisionLayers;

    [Header("Movement")]
    [Tooltip("Maximum ground speed when moving.")]
    public float WalkSpeed = 5f;

    [Tooltip("Maximum speed when sprinting.")]
    public float SprintSpeed = 7f;

    [Tooltip("Maximum change in velocity per physics step.")]
    public float MaxVelocityChange = 10f;

    [Header("Jump")]
    [Tooltip("Enable/disable jumping.")]
    public bool EnableJump = true;

    [Tooltip("Upward impulse applied when jumping.")]
    public float JumpPower = 5f;
    [Tooltip("Grace window after leaving ground in which a jump still succeeds.")]
    public float CoyoteTime = 0.15f;

    [Tooltip("Window before landing in which a jump press will be buffered.")]
    public float JumpBufferTime = 0.25f;

    [Tooltip("Multiplier for gravity when jump is ended early")]
    public float EndJumpEarlyExtraForceMultiplier = 3f;

    [Tooltip("Maximum number of air jumps (excluding initial and coyote)")]
    public int MaxAirJumps = 1;

    [Tooltip("If true, gives the player more control at the apex of a jump")]
    public bool UseApexControl = true;

    [Tooltip("Multiplier applied to horizontal control at the apex")]
    [Range(0f, 5f)] public float ApexModifier = 1.5f;

    [Tooltip("Only triggers ApexControl when |velocity.y| is below this value")]
    [Range(0f, 10f)] public float ApexDetectionThreshold = 2f;

    [Header("Grounding")]
    [Tooltip("Extra distance for ground raycast.")]
    public float GroundCheckDistance = 0.75f;
}
