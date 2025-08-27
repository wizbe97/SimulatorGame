using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private CameraControllerSO cameraSettings;

    private PlayerInputHandler inputHandler;
    private CinemachinePOV pov;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (vcam == null) vcam = FindObjectOfType<CinemachineVirtualCamera>();

        BindPlayerCameraRig();
        cameraSettings?.ConfigureVcam(vcam);

        pov = vcam != null ? vcam.GetCinemachineComponent<CinemachinePOV>() : null;
        inputHandler = player != null ? player.GetComponent<PlayerInputHandler>() : null;
    }

    private void Update()
    {
        if (pov == null || inputHandler == null || cameraSettings == null) return;

        Vector2 look = inputHandler.LookInput;
        float delta = cameraSettings.MouseSensitivity * 2f * Time.deltaTime;

        pov.m_HorizontalAxis.Value += look.x * delta;

        float ySign = cameraSettings.InvertY ? 1f : -1f;
        pov.m_VerticalAxis.Value += look.y * delta * ySign;
    }

    private void BindPlayerCameraRig()
    {
        if (vcam == null || player == null) return;

        var followTarget = player.transform.Find("Goggles/CameraFollow");
        if (followTarget == null)
        {
            Debug.LogError("CameraManager: 'Goggles/CameraFollow' not found under Player.");
            return;
        }

        vcam.Follow = followTarget;
        vcam.LookAt = followTarget;
    }

    public void RebindToPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        inputHandler = player != null ? player.GetComponent<PlayerInputHandler>() : null;
        BindPlayerCameraRig();
        cameraSettings?.ConfigureVcam(vcam);
        pov = vcam != null ? vcam.GetCinemachineComponent<CinemachinePOV>() : null;
    }
}
