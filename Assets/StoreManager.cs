using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{
    [SerializeField] private int UpgradePrice;
    [SerializeField] private int secoundsToReward;
    [SerializeField] private int RewardAmount;
    [SerializeField] private GameObject buySign;
    [SerializeField] private TMP_Text PriceText;
    [SerializeField] private GameObject oldStore;
    [SerializeField] private GameObject newStore;
    [SerializeField] private ParticleSystem cointEffect;

    private void Start()
    {
        PriceText.text = UpgradePrice.ToString();
    }

    public void OnClickBuy()
    {
        CurrencyWallet wallet = CurrencyWallet.Instance;
        if(wallet.Coins >= UpgradePrice)
        {
            oldStore.SetActive(false);
            newStore.SetActive(true);
            wallet.UpdateCoins(wallet.Coins - UpgradePrice);
            StartCoroutine(GiveCoins());
            buySign.SetActive(false);
        }
    }

    IEnumerator GiveCoins()
    {
        CurrencyWallet.Instance.UpdateCoins(CurrencyWallet.Instance.Coins + RewardAmount);
        cointEffect?.Play();
        yield return new WaitForSeconds(secoundsToReward);
        StartCoroutine(GiveCoins());
    }
}
