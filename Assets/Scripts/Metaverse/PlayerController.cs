using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;      // grace after leaving ground
    public float jumpBufferTime = 0.1f;  // grace before landing
    public float lowJumpMultiplier = 4f; // better jump feel
    public float fallMultiplier = 6f;    // stronger fall

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    private Rigidbody2D _rb;
    private bool _isGrounded;
    private float _coyoteTimer;
    private float _jumpBufferTimer;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        Vector2 v = _rb.linearVelocity;
        v.x = x * moveSpeed;
        _rb.linearVelocity = v;

        if (x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }

        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);
        if (_isGrounded)
            _coyoteTimer = coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            _jumpBufferTimer = jumpBufferTime;
        else
            _jumpBufferTimer -= Time.deltaTime;

        if (_jumpBufferTimer > 0 && _coyoteTimer > 0)
        {
            _jumpBufferTimer = 0;
            _coyoteTimer = 0;
            Jump();
        }

        if (_rb.linearVelocity.y < 0)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        else if (_rb.linearVelocity.y > 0)
            _rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
    }

    private void Jump()
    {
        Vector2 v = _rb.linearVelocity;
        v.y = jumpForce;
        _rb.linearVelocity = v;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
