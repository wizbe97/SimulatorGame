using UnityEngine.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [Header("Prefabs")]
    [SerializeField] private GameObject[] menuManagerPrefabs;
    [SerializeField] private GameObject[] gameplayManagerPrefabs;

    [Header("Settings")]
    [Header("Debug Variables")]
    private bool isOnMenu;

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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Menu")
            isOnMenu = true;
        else
            isOnMenu = false;

        SpawnManagers(isOnMenu);
    }

    private void SpawnManagers(bool isOnMenu)
    {
        if (isOnMenu)
            foreach (var prefab in menuManagerPrefabs)
                Instantiate(prefab);
        else
            foreach (var prefab in gameplayManagerPrefabs)
                Instantiate(prefab);
    }
}
