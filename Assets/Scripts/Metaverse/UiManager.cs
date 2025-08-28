using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance;
    public TMP_Text coinText;
    public TMP_Text gemText;

    void Awake()
    {
        Instance = this;
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
}
