using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    public HealthSystem healthSystem;
    public TMP_Text coinText;
    public TMP_Text gemText;

    [Header("WinPanal Referance")]
    public GameObject WinPanal;
    public TMP_Text WinCoinText;
    public TMP_Text WinGemText;
    public TMP_Text WinHeartLeftText;
    public GameObject[] WinStarts;

    [Header("LosePanal Referance")]
    public GameObject LosePanal;
    public TMP_Text LoseCoinText;
    public TMP_Text LoseGemText;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        coinText.text = "0";
        gemText.text = "0";
    }

    public void UpdateCoins(int amount)
    {
        coinText.text = $"{amount}";
    }


    public void UpdateGems(int amount)
    {
        gemText.text = $"{amount}";
    }

    public void WinEndGame()
    {
        WinPanal.SetActive(true);
        WinCoinText.text = CurrencyWallet.Instance.Coins.ToString();
        WinGemText.text = CurrencyWallet.Instance.Gems.ToString();
        WinHeartLeftText.text = healthSystem != null ? healthSystem.currentLives.ToString() : "0";
        for(int i = 0; i < healthSystem.currentLives; i++)
        {
            WinStarts[i].SetActive(true);
        }
    }

    public void LoseEndGame()
    {
        LosePanal.SetActive(true);
        LoseCoinText.text = CurrencyWallet.Instance.Coins.ToString();
        LoseGemText.text = CurrencyWallet.Instance.Gems.ToString();
    }
}
