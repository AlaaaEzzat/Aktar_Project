using UnityEngine;

public class MovingProjectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float m_Speed = 10f;
    [SerializeField] private float lifetime = 5f;

    private Rigidbody2D rb;
    private int direction = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Destroy(gameObject, lifetime);
    }

    public void SetDirection(int dir)
    {
        direction = dir;
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * m_Speed, rb.linearVelocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (collision.TryGetComponent<ShootingEnemy>(out var enemy))
            {
                enemy.TakeDamage();
            }
            Destroy(gameObject);
        }
    }
}
