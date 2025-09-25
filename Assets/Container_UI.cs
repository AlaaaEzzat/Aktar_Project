using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Container_UI : MonoBehaviour
{
    public List<ItemSO> items = new List<ItemSO>();
    public TMPro.TMP_Text priceText;
    public Image currencyImage;
    public Image itemToShow;
    public Image LockedIcon;
    public Button NexItem;
    public Button PreviousItem;
    public Button BuyButton;
    public Button UseButton;
    public Button UsedButton;
    public Sprite coinsImage;
    public Sprite gemsImage;
    private int startIndex;
    private bool canBuy = false;
    private CurrencyWallet wallet;
    private void Start()
    {
        wallet = CurrencyWallet.Instance;
        startIndex = 0;
        NexItem.onClick.AddListener(GetNextItem);
        PreviousItem.onClick.AddListener(GetPrivousItem);
        BuyButton.onClick.AddListener(BuyItem);
        UseButton.onClick.AddListener(UseItem);
        UsedButton.onClick.AddListener(UnUseItem);
        ChangeShowedItem();
    }

    public void GetNextItem()
    {
        startIndex++;
        if (startIndex > items.Count - 1)
        {
            startIndex = items.Count - 1;
        }
        ChangeShowedItem();
    }

    public void GetPrivousItem()
    {
        startIndex--;
        if(startIndex < 0)
        {
            startIndex = 0;
        }
        ChangeShowedItem();
    }

    public void BuyItem()
    {
        if (canBuy)
        {
            ItemSO currentItem = items[startIndex];
            if (currentItem.currencyType == CurrencyType.Coins)
            {
                wallet.AddCoins(-currentItem.price);
                LockedIcon.gameObject.SetActive(false);
                BuyButton.gameObject.SetActive(false);
            }
            else
            {
                wallet.AddCoins(- currentItem.price);
                LockedIcon.gameObject.SetActive(false);
                BuyButton.gameObject.SetActive(false);
            }
            wallet.OwnedItems.Add(currentItem);
            UseButton.gameObject.SetActive(true);
        }
    }

    public void ChangeShowedItem()
    {
        bool owned = wallet.OwnedItems.FirstOrDefault(item => item.itemId == items[startIndex].itemId);
        itemToShow.sprite = items[startIndex].icon;
        ItemSO currentItem = items[startIndex];

        if (owned)
        {
            if (currentItem.name.Contains("Cap"))
            {
                UseButton.gameObject.SetActive(wallet.headItem == null);
                UsedButton.gameObject.SetActive(wallet.headItem != null && wallet.headItem.itemId == currentItem.itemId);
            }
            else
            {
                UseButton.gameObject.SetActive(wallet.ringItem == null);
                UsedButton.gameObject.SetActive(wallet.ringItem != null &&  wallet.ringItem.itemId == currentItem.itemId);
            }

        }
        else
        {
            UseButton.gameObject.SetActive(false);
            UsedButton.gameObject.SetActive(false);
        }
        priceText.text = currentItem.price.ToString();
        if (currentItem.currencyType == CurrencyType.Coins)
        {
            currencyImage.sprite = coinsImage;
            LockedIcon.gameObject.SetActive(wallet.Coins < currentItem.price && !owned);
            canBuy = wallet.Coins > currentItem.price ? true : false;
        }
        else
        {
            currencyImage.sprite = gemsImage;
            LockedIcon.gameObject.SetActive(wallet.Gems < currentItem.price && !owned);
            canBuy = wallet.Gems > currentItem.price ? true : false;
        }

        BuyButton.gameObject.SetActive(!owned);
    }

    public void UseItem()
    {
        UsedButton.gameObject.SetActive(true);
        if (items[startIndex].name.Contains("Cap"))
        {
            wallet.headItem = items[startIndex];
        }
        else
        {
            wallet.ringItem = items[startIndex];
        }
        UseButton.gameObject.SetActive(false);

    }
    public void UnUseItem()
    {
        UseButton.gameObject.SetActive(true);
        if (items[startIndex].name.Contains("Cap"))
        {
            wallet.headItem = null;
        }
        else
        {
            wallet.ringItem = null;
        }
        UsedButton.gameObject.SetActive(false);
    }
}
