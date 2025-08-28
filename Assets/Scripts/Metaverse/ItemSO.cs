using UnityEngine;

[CreateAssetMenu(menuName = "Game/Item", fileName = "Item_ ")]
public class ItemSO : ScriptableObject
{
    public string itemId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
}
