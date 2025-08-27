using UnityEngine;
using Cinemachine;

[CreateAssetMenu(menuName = "Game/Camera Controller")]
public class CameraControllerSO : ScriptableObject
{
    [Header("Body Mode")]
    public BodyMode Body = BodyMode.HardLockToTarget;

    [Header("Look / POV")]
    public float MouseSensitivity = 300f;
    public float MaxLookAngle = 90f;
    public bool InvertY = false;
    public bool WrapYaw = true;

    public enum BodyMode { HardLockToTarget }

    public void ConfigureVcam(CinemachineVirtualCamera vcam)
    {
        if (!vcam) return;

        var hardLock = vcam.GetCinemachineComponent<CinemachineHardLockToTarget>();
        if (hardLock == null)
            hardLock = vcam.AddCinemachineComponent<CinemachineHardLockToTarget>();

        var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        if (pov == null)
            pov = vcam.AddCinemachineComponent<CinemachinePOV>();

        // Clamp & wrap
        pov.m_HorizontalAxis.m_Wrap = WrapYaw;
        pov.m_VerticalAxis.m_MinValue = -MaxLookAngle;
        pov.m_VerticalAxis.m_MaxValue =  MaxLookAngle;
        pov.m_VerticalAxis.m_Wrap = false;

        // Disable smoothing & recentering to prevent “bounce/snap”
        pov.m_HorizontalAxis.m_AccelTime = 0f;
        pov.m_HorizontalAxis.m_DecelTime = 0f;
        pov.m_VerticalAxis.m_AccelTime   = 0f;
        pov.m_VerticalAxis.m_DecelTime   = 0f;
        pov.m_HorizontalRecentering.m_enabled = false;
        pov.m_VerticalRecentering.m_enabled   = false;

        var provider = vcam.GetComponent<CinemachineInputProvider>();
        if (provider) provider.enabled = false;
    }
}
