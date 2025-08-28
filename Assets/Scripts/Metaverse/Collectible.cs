using UnityEngine;

public enum CollectibleType { Coin, Gem, Item }

public class Collectible : MonoBehaviour
{
    public CollectibleType type = CollectibleType.Coin;
    public int amount = 1; // for coins/gems
    public ItemSO item;    // used when type == Item

    private Transform player;
    private bool magnaticActivated;

    private void Update()
    {
        if (magnaticActivated && player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position , 10f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var wallet = other.GetComponent<CurrencyWallet>();
            var inventory = other.GetComponent<Inventory>();

            switch (type)
            {
                case CollectibleType.Coin:
                    wallet?.AddCoins(amount);
                    break;
                case CollectibleType.Gem:
                    wallet?.AddGems(amount);
                    break;
                case CollectibleType.Item:
                    if (item != null) inventory?.AddItem(item);
                    break;
            }

            // Optional: play SFX/VFX here
            Destroy(gameObject);
        }
        else if(other.CompareTag("Magnatic"))
        {
            player = other.gameObject.transform;
            magnaticActivated = true;
        }


    }
}
