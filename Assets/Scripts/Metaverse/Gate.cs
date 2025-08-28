using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] private string requiredItemId = "K1";
    [SerializeField] private GameObject doorLockIcon;

    private Inventory playerInventory;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
           playerInventory = collision.GetComponent<Inventory>();
            if (playerInventory.HasItem(requiredItemId))
            {
                OpenDoor();
            }
            else
            {
                Debug.Log("You need the golden key to open this door!");
            }
        }
    }

    private void OpenDoor()
    {
        animator.enabled = true;
        doorLockIcon.SetActive(false);
    }
}
