using UnityEngine;

[CreateAssetMenu(menuName = "Game/Camera Controller")]
public class CameraControllerSO : ScriptableObject
{
    [Header("Look")]
    [Tooltip("Mouse sensitivity multiplier.")]
    public float MouseSensitivity = 2f;

    [Tooltip("Maximum up/down look angle in degrees.")]
    public float MaxLookAngle = 90f;

    [Header("Cursor")]
    public bool LockCursorOnStart = true;

    [Header("Offsets")]
    [Tooltip("Optional pivot or local offset for the camera relative to the player.")]
    public Vector3 CameraLocalOffset = new Vector3(0f, 0.6f, 0f);
}
