using System;
using UnityEngine;

public class CurrencyWallet : MonoBehaviour
{
    public int Coins { get; private set; }
    public int Gems { get; private set; }

    public event Action<int> OnCoinsChanged;
    public event Action<int> OnGemsChanged;

    public void AddCoins(int amount)
    {
        Coins = Mathf.Max(0, Coins + amount);
        UiManager.Instance.UpdateCoins(Coins);
    }

    public void AddGems(int amount)
    {
        Gems = Mathf.Max(0, Gems + amount);
        UiManager.Instance.UpdateGems(Gems);
    }

    public void Set(int coins, int gems)
    {
        Coins = Mathf.Max(0, coins);
        Gems = Mathf.Max(0, gems);
        OnCoinsChanged?.Invoke(Coins);
        OnGemsChanged?.Invoke(Gems);
    }
}
