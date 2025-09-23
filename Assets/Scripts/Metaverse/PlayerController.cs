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

    [Header("Attacking")]
    public GameObject projectileObject;
    public Transform shootingPoint;

    private Rigidbody2D _rb;
    private Inventory _inventory;
    private Animator _anim;
    private bool _isGrounded;
    private float _coyoteTimer;
    private float _jumpBufferTimer;
    private PlayerState _currentState = PlayerState.Idle;
    private bool isAttacing;

    private Vector2 _moveInput;
    private bool _jumpPressed;
    private bool _attackPressed;
    public bool IsJumpHeld { get; private set; }

    private PlayerControlls _controls;

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

    public Animator Animator => _anim;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _inventory = GetComponent<Inventory>();
        _anim = GetComponent<Animator>();
        _inventory.OnItemCollected += EnableAttackMode;

        _controls = new PlayerControlls();

        // Movement input
        _controls.Player.Move.performed += ctx => _moveInput = ctx.ReadValue<Vector2>();
        _controls.Player.Move.canceled += ctx => _moveInput = Vector2.zero;

        // Jump input
        _controls.Player.Jump.performed += ctx => _jumpPressed = true;
        _controls.Player.Jump.canceled += ctx => IsJumpHeld = false;
        _controls.Player.Jump.performed += ctx => IsJumpHeld = true;

        // Attack input
        _controls.Player.Attack.performed += ctx => _attackPressed = true;

        isAttacing = false;
    }

    private void OnEnable()
    {
        _controls.Enable();
    }

    private void OnDisable()
    {
        _controls.Disable();
    }

    private void Update()
    {
        HandleMovement();
        HandleJump();
        HandleAttack();
    }

    private void HandleMovement()
    {
        Vector2 v = _rb.linearVelocity;
        v.x = _moveInput.x * moveSpeed;
        _rb.linearVelocity = v;

        if (_moveInput.x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(_moveInput.x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    private void HandleJump()
    {
        if (_currentState == PlayerState.Flying) return;

        _isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundMask);

        if (_isGrounded)
            _coyoteTimer = coyoteTime;
        else
            _coyoteTimer -= Time.deltaTime;

        if (_jumpPressed)
        {
            _jumpBufferTimer = jumpBufferTime;
            _jumpPressed = false;
        }
        else
        {
            _jumpBufferTimer -= Time.deltaTime;
        }

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

    private void HandleAttack()
    {
        if (_currentState != PlayerState.Attacking || !_attackPressed || isAttacing)
            return;

        _anim.SetTrigger("Attack");
        GameObject obj = Instantiate(projectileObject, shootingPoint.position, Quaternion.identity);

        if (obj.TryGetComponent<MovingProjectile>(out var projectile))
        {
            int direction = transform.localScale.x > 0 ? 1 : -1;
            projectile.SetDirection(direction);
        }

        isAttacing = true;
        _attackPressed = false;
        Invoke("EndAttack", 0.2f);
    }

    private void EndAttack()
    {
        isAttacing = false;
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
        if (item.itemId == "Fist")
        {
            GetComponent<JetPack>().DisableJetpack();
            _currentState = PlayerState.Attacking;
            _anim.SetBool("AttackingMode", true);
        }
    }

    public void DissableAttackMode()
    {
        _currentState = PlayerState.Idle;
        _anim.SetBool("AttackingMode", false);
    }
}