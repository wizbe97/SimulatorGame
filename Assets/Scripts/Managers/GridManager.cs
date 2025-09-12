using UnityEngine;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private float gridSize = 0.5f;  // smaller = smoother snapping

    public float GridSize => gridSize;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public Vector3 SnapToGrid(Vector3 worldPos)
    {
        float size = gridSize;
        worldPos.x = Mathf.Round(worldPos.x / size) * size;
        worldPos.z = Mathf.Round(worldPos.z / size) * size;
        return worldPos;
    }
}
