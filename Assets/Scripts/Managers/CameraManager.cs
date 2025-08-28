using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public static CameraManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject player;
    [SerializeField] private CinemachineVirtualCamera vcam;
    [SerializeField] private CameraSettingsSO cameraSettings;

    [Header("Follow Target (under player)")]
    [SerializeField] private string followTargetPath = "Goggles/CameraFollow";

    private PlayerInputHandler inputHandler;
    private CinemachinePOV pov;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start() => Bind();

    private void Update()
    {
        if (pov == null || inputHandler == null || cameraSettings == null) return;

        Vector2 look = inputHandler.LookInput;
        float delta = inputHandler.GetLookSensitivity(cameraSettings) * 2f * Time.deltaTime;

        pov.m_HorizontalAxis.Value += look.x * delta;

        float ySign = cameraSettings.InvertY ? 1f : -1f;
        pov.m_VerticalAxis.Value += look.y * delta * ySign;
    }

    private void Bind()
    {
        if (vcam == null) vcam = FindObjectOfType<CinemachineVirtualCamera>();
        if (player == null || vcam == null) return;

        inputHandler = player.GetComponent<PlayerInputHandler>();
        pov = vcam.GetCinemachineComponent<CinemachinePOV>();
        cameraSettings?.ConfigureVcam(vcam);

        Transform follow = string.IsNullOrEmpty(followTargetPath)
            ? null
            : player.transform.Find(followTargetPath);

        vcam.Follow = follow ? follow : player.transform;
        vcam.LookAt = vcam.Follow;
    }

    public void RebindToPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        Bind();
    }
}
