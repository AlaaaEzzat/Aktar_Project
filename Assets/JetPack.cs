using UnityEngine;

public class JetPack : MonoBehaviour
{
    private Inventory playerInventory;
    private Animator anim;
    private PlayerController playerController;
    private Rigidbody2D rb;

    public float thrust = 8f;
    public float gravityScale = 2.5f;



    private void Start()
    {
        playerInventory = gameObject.GetComponent<Inventory>();
        anim = gameObject.GetComponent<Animator>();
        playerController = gameObject.GetComponent<PlayerController>();
        rb = gameObject.GetComponent<Rigidbody2D>();

        playerInventory.OnItemCollected += HandleItemCollected;

    }

    private void OnDisable()
    {
        playerInventory.OnItemCollected -= HandleItemCollected;
    }

    private void Update()
    {
        if(playerController.CurrentState != PlayerState.Flying)
            return;

        if (Input.GetButton("Jump"))
        {
            rb.AddForce(Vector2.up * thrust);

        }

        rb.linearVelocity += Vector2.up * Physics2D.gravity.y * gravityScale * Time.deltaTime;
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
        playerController.CurrentState = PlayerState.Flying;
        anim.SetBool("FlyingMode", true);
    }

    public void DissableJetpack()
    {
        playerController.CurrentState = PlayerState.Idle;
        anim.SetBool("FlyingMode", false);
    }
}
