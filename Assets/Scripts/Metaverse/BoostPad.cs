using UnityEngine;

public class BoostPad : MonoBehaviour
{
    public float boostForceY = 15f;
    private Animator animator;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, boostForceY);

                if (animator != null)
                    animator.SetTrigger("Play");
            }
        }
    }
}
