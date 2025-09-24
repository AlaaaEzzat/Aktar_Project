using System.Collections.Generic;
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
    public Sprite coinsImage;
    public Sprite gemsImage;
    private int startIndex;
    private bool canBuy = false;

    private void Start()
    {
        startIndex = 0;
        NexItem.onClick.AddListener(GetNextItem);
        PreviousItem.onClick.AddListener(GetPrivousItem);
        BuyButton.onClick.AddListener(BuyItem);
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
            CurrencyWallet w = CurrencyWallet.Instance;
            ItemSO currentItem = items[startIndex];
            if (currentItem.currencyType == CurrencyType.Coins)
            {
                w.AddCoins(w.Coins - currentItem.price);
                LockedIcon.gameObject.SetActive(false);
                BuyButton.gameObject.SetActive(false);
            }
            else
            {
                w.AddCoins(w.Gems - currentItem.price);
                LockedIcon.gameObject.SetActive(false);
                BuyButton.gameObject.SetActive(false);
            }
        }
    }

    public void ChangeShowedItem()
    {
        itemToShow.sprite = items[startIndex].icon;
        CurrencyWallet w = CurrencyWallet.Instance;
        ItemSO currentItem = items[startIndex];
        priceText.text = currentItem.price.ToString();
        if (currentItem.currencyType == CurrencyType.Coins)
        {
            currencyImage.sprite = coinsImage;
            LockedIcon.gameObject.SetActive(w.Coins < currentItem.price);
            canBuy = w.Coins > currentItem.price ? true : false;
        }
        else
        {
            currencyImage.sprite = gemsImage;
            LockedIcon.gameObject.SetActive(w.Gems < currentItem.price);
            canBuy = w.Gems > currentItem.price ? true : false;
        }

        BuyButton.gameObject.SetActive(/*canBuy &&*/ ! w.OwnedItems.Contains(items[startIndex]));
    }
}
