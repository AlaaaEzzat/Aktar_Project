using UnityEngine;

public enum CurrencyType
{
    Coins,
    Nft
}
[CreateAssetMenu(menuName = "Game/Item", fileName = "Item_ ")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string displayName;
    public CurrencyType currencyType;
    public int price;
    public bool owned = false;
    [TextArea] public string description;
    public Sprite icon;
    public Sprite Gameplayicon;
    public Color GameplayItemIconColor;
}
