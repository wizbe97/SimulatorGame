using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    private GameObject currentPlayer;

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

    private void Start()
    {
        SpawnPlayer();
    }

    public void SpawnPlayer()
    {
        if (currentPlayer != null) return;

        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        CameraManager.Instance?.RebindToPlayer(currentPlayer);
    }

    public GameObject GetPlayer()
    {
        return currentPlayer;
    }
}
