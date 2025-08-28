using UnityEngine;
using Cinemachine;

[CreateAssetMenu(menuName = "Game/Camera Settings")]
public class CameraSettingsSO : ScriptableObject
{
    [Header("Look / POV")]
    public float MouseSensitivity = 10f;
    public float ControllerSensitivity = 150f;
    public float MaxLookAngle = 80f;
    public bool InvertY = false;
    public bool WrapYaw = true;

    public void ConfigureVcam(CinemachineVirtualCamera vcam)
    {
        if (!vcam) return;

        var hardLock = vcam.GetCinemachineComponent<CinemachineHardLockToTarget>();
        if (hardLock == null) _ = vcam.AddCinemachineComponent<CinemachineHardLockToTarget>();

        var pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        if (pov == null) pov = vcam.AddCinemachineComponent<CinemachinePOV>();

        pov.m_HorizontalAxis.m_Wrap = WrapYaw;
        pov.m_VerticalAxis.m_MinValue = -MaxLookAngle;
        pov.m_VerticalAxis.m_MaxValue =  MaxLookAngle;
        pov.m_VerticalAxis.m_Wrap = false;
    }
}
