using UnityEngine;

public class JetPack : MonoBehaviour
{
    private Inventory playerInventory;
    private Animator anim;
    private PlayerController playerController;
    private Rigidbody2D rb;

    [Header("Jetpack Settings")]
    public float thrust = 8f;
    public float gravityScale = 2.5f;
    public float jetpackGravityScale = 0.5f;

    private bool hasJetpack = false;

    private void Start()
    {
        playerInventory = GetComponent<Inventory>();
        anim = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (playerInventory != null)
            playerInventory.OnItemCollected += HandleItemCollected;
    }

    private void OnDisable()
    {
        if (playerInventory != null)
            playerInventory.OnItemCollected -= HandleItemCollected;
    }

    private void Update()
    {
        if (!hasJetpack || playerController.CurrentState != PlayerState.Flying)
            return;

        HandleJetpackMovement();
    }

    private void HandleJetpackMovement()
    {
        if (Input.GetButton("Jump"))
        {
            rb.AddForce(Vector2.up * thrust, ForceMode2D.Force);
        }

        rb.gravityScale = jetpackGravityScale;
    }

    private void HandleItemCollected(ItemSO item)
    {
        if (item.itemId == "Jet")
        {
            Debug.Log("Jetpack collected! Jetpack mode activated!");
            EnableJetpack();
        }
    }

    private void EnableJetpack()
    {
        hasJetpack = true;
        playerController.CurrentState = PlayerState.Flying;
        rb.gravityScale = jetpackGravityScale;
        anim.SetBool("FlyingMode", true);
    }

    public void DisableJetpack()
    {
        hasJetpack = false;
        playerController.CurrentState = PlayerState.Idle;
        rb.gravityScale = gravityScale;
        anim.SetBool("FlyingMode", false);
    }
}
