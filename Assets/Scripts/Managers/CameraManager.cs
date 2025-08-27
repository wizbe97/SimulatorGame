using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
public class CameraManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraControllerSO cameraSettings;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Transform playerRoot; // yaw target (usually the player root)

    private PlayerInputHandler inputHandler;
    private float yaw;
    private float pitch;

    private void Awake()
    {
        inputHandler = GetComponent<PlayerInputHandler>();
        if (playerRoot == null) playerRoot = transform;

        if (playerCamera != null)
        {
            // Apply SO-defined local offset (if any)
            playerCamera.transform.localPosition = cameraSettings != null
                ? cameraSettings.CameraLocalOffset
                : Vector3.zero;
        }

        // Initial cursor state via CursorManager
        if (cameraSettings != null && cameraSettings.LockCursorOnStart)
        {
            CursorManager.Instance?.SetCursorLock(true);
        }
    }

    private void Update()
    {
        if (cameraSettings == null || playerCamera == null || inputHandler == null)
            return;

        Vector2 look = inputHandler.LookInput;

        float scaledSensitivity = cameraSettings.MouseSensitivity * Time.deltaTime;
        yaw   += look.x * scaledSensitivity;
        pitch -= look.y * scaledSensitivity;

        pitch = Mathf.Clamp(pitch, -cameraSettings.MaxLookAngle, cameraSettings.MaxLookAngle);

        // Apply rotations: yaw to player, pitch to camera
        if (playerRoot != null)
            playerRoot.rotation = Quaternion.Euler(0f, yaw, 0f);

        playerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

}
