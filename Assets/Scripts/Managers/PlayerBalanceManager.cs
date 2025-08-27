using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBalanceManager : MonoBehaviour
{
    public static PlayerBalanceManager Instance { get; private set; }
    public PlayerBalanceSO playerBalance;
    public GameEventSO onBalanceChangedEvent;

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

    public bool CanAfford(int amount)
    {
        return playerBalance.balance >= amount;
    }

    public void DeductBalance(int amount)
    {
        if (CanAfford(amount))
        {
            playerBalance.balance -= amount;
            if (onBalanceChangedEvent != null)
                onBalanceChangedEvent.Raise();

        }
    }

    public void AddBalance(int amount)
    {
        playerBalance.balance += amount;

        if (onBalanceChangedEvent != null)
            onBalanceChangedEvent.Raise();
    }

    public void ClearBalance()
    {
        playerBalance.balance = playerBalance.startingBalace;
        if (onBalanceChangedEvent != null)
            onBalanceChangedEvent.Raise();
    }
}
