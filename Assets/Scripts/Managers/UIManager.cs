using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }
    [SerializeField] private TMP_Text _balanceText;


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
        UpdateBalanceUI();
    }

    public void UpdateBalanceUI()
    {
        _balanceText.text = "Balance: $" + PlayerBalanceManager.Instance.playerBalance.balance;
    }
}
