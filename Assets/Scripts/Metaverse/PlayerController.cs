using UnityEngine;

public enum PlayerState
{
    Idle,
    Flying,
    Attacking
}
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 8f;

    [Header("Jumping")]
    public float jumpForce = 14f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;
    public float lowJumpMultiplier = 4f;
    public float fallMultiplier = 6f; 

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.15f;
    public LayerMask groundMask;

    private Rigidbody2D _rb;
    private Inventory _inventory;
    private Animator _anim;
    private bool _isGrounded;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private PlayerState _currentState = PlayerState.Idle;
    private bool isAttacing;

    public PlayerState CurrentState
    {
        get { return _currentState; }
        set { _currentState = value; }
    }

    public bool IsAttacking
    {
        get { return isAttacing; }
        set { isAttacing = value; }
    }

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<Inventory>();
        _anim = GetComponent<Animator>();
        _inventory.OnItemCollected += EnableAttackMode;
    }

    private void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        Vector2 v = _rb.linearVelocity;
        v.x = x * moveSpeed;
        _rb.linearVelocity = v;

        Attack();

        if (x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }

        if (_currentState != PlayerState.Flying)
        {
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
    }

    private void Jump()
    {
        Vector2 v = _rb.linearVelocity;
        v.y = jumpForce;
        _rb.linearVelocity = v;
    }

    private void Attack()
    {
        if(_currentState != PlayerState.Attacking)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            _anim.SetTrigger("Attack");
            isAttacing = true;
            Invoke("EndAttack", 0.2f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }

    private void EnableAttackMode(ItemSO item)
    {
        if(item.itemId == "Fist")
        {
            _currentState = PlayerState.Attacking;
            _anim.SetBool("AttackingMode", true); 
        }
    }

    private void EndAttack()
    {
        isAttacing = false;
    }
}
