using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController2D : MonoBehaviour
{
	[Header("Tilemap Settings")]
	public Tilemap wallTilemap;

	[Header("Movement")]
	public float moveSpeed = 7f;

	[Header("Area Visual Debug")]
	public float gizmoBoxScale = 1.0f;
	public Vector2Int areaRadius = new Vector2Int(1, 1);

	[Header("Sprites & Animation")]
	public SpriteRenderer spriteRenderer;
	public Sprite[] upIdleSprites, downIdleSprites, sideIdleSprites;
	public Sprite[] upWalkSprites, downWalkSprites, sideWalkSprites;
	public float walkFrameRate = 0.12f, idleFrameRate = 0.22f;

	[Header("Additional Collision Settings")]
	public LayerMask obstacleLayer;
	public float obstacleCheckRadius = 0.1f;

	private Rigidbody2D rb;
	private Vector3Int gridDirection = Vector3Int.zero;
	private Vector3Int bufferedDirection = Vector3Int.zero;
	private Vector3Int mobileInputDirection = Vector3Int.zero;

	private Vector3 targetWorldPosition;
	private bool isMoving = false;
	private string lastDirection = "down";
	private int animFrame = 0;
	private float animTimer = 0f;
	private bool prevMoving = false;
	private string prevDirection = "down";
	private bool movementLocked = false;
	private Vector3 startPosition;

	[Header("Sound Settings")]
	public string moveSoundKey = "move";
	public string resetSoundKey = "reset";
	private bool isMoveSoundPlaying = false; // NEW

	public static PlayerController2D Instance { get; private set; }

	public bool isWin = false;
	void Awake()
	{
		Instance = this;

		rb = GetComponent<Rigidbody2D>();
		var cell = wallTilemap.WorldToCell(transform.position);
		transform.position = wallTilemap.GetCellCenterWorld(cell);
		targetWorldPosition = transform.position;
		startPosition = transform.position;
		SetIdleSprite();
	}

	void Update()
	{
		if (movementLocked)
		{
			rb.linearVelocity = Vector2.zero;
			isMoving = false;
			StopMoveSound();
			return;
		}

		HandleInput();

		if (!isMoving && bufferedDirection != Vector3Int.zero && CanMoveInDirection(bufferedDirection))
		{
			gridDirection = bufferedDirection;
			lastDirection = DirToString(gridDirection);
			bufferedDirection = Vector3Int.zero;
		}

		if (!isMoving && gridDirection != Vector3Int.zero && CanMoveInDirection(gridDirection))
		{
			var currCell = wallTilemap.WorldToCell(transform.position);
			var nextCell = currCell + gridDirection;
			targetWorldPosition = wallTilemap.GetCellCenterWorld(nextCell);
			isMoving = true;
			PlayMoveSoundIfNeeded();
		}

		AnimateSprite();
	}

	void FixedUpdate()
	{
		if (movementLocked)
		{
			rb.linearVelocity = Vector2.zero;
			StopMoveSound();
			return;
		}

		if (isMoving)
		{
			Vector2 pos = Vector2.MoveTowards(transform.position, targetWorldPosition, moveSpeed * wallTilemap.cellSize.x * Time.fixedDeltaTime);
			rb.MovePosition(pos);

			if ((Vector2)transform.position == (Vector2)targetWorldPosition)
			{
				isMoving = false;
				StopMoveSound();

				if (bufferedDirection != Vector3Int.zero && CanMoveInDirection(bufferedDirection))
				{
					gridDirection = bufferedDirection;
					lastDirection = DirToString(gridDirection);
					bufferedDirection = Vector3Int.zero;
				}
				else if (!CanMoveInDirection(gridDirection))
				{
					gridDirection = Vector3Int.zero;
				}
			}
		}
		else
		{
			// Check if we can start moving again
			if (gridDirection != Vector3Int.zero && CanMoveInDirection(gridDirection))
			{
				var currCell = wallTilemap.WorldToCell(transform.position);
				var nextCell = currCell + gridDirection;
				targetWorldPosition = wallTilemap.GetCellCenterWorld(nextCell);
				isMoving = true;
				PlayMoveSoundIfNeeded();
			}
			else
			{
				rb.linearVelocity = Vector2.zero;
				StopMoveSound();
			}
		}
	}

	void HandleInput()
	{
		if (movementLocked) return;

		// Keyboard input
		if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) bufferedDirection = Vector3Int.up;
		if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) bufferedDirection = Vector3Int.down;
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) bufferedDirection = Vector3Int.left;
		if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) bufferedDirection = Vector3Int.right;

		// Mobile UI input
		if (!isMoving && mobileInputDirection != Vector3Int.zero)
		{
			bufferedDirection = mobileInputDirection;
		}
	}

	bool CanMoveInDirection(Vector3Int dir)
	{
		if (dir == Vector3Int.zero) return false;

		Vector3Int currCell = wallTilemap.WorldToCell(transform.position);
		Vector3Int nextCell = currCell + dir;

		if (wallTilemap.HasTile(nextCell))
			return false;

		Vector3 nextWorldPos = wallTilemap.GetCellCenterWorld(nextCell);
		if (Physics2D.OverlapCircle(nextWorldPos, obstacleCheckRadius, obstacleLayer))
			return false;

		return true;
	}

	void AnimateSprite()
	{
		if (movementLocked)
			return;

		Sprite[] sprites = GetCurrentAnimationArray();
		float targetFrameRate = isMoving ? walkFrameRate : idleFrameRate;

		if (prevMoving != isMoving || prevDirection != lastDirection)
		{
			animFrame = 0;
			animTimer = 0f;
		}
		prevMoving = isMoving;
		prevDirection = lastDirection;

		if (sprites == null || sprites.Length == 0)
			return;

		animTimer += Time.deltaTime;
		if (animTimer >= targetFrameRate)
		{
			animTimer = 0f;
			animFrame = (animFrame + 1) % sprites.Length;
		}
		spriteRenderer.sprite = sprites[animFrame];

		if (lastDirection == "right")
			spriteRenderer.flipX = true;
		else if (lastDirection == "left")
			spriteRenderer.flipX = false;
	}

	void SetIdleSprite()
	{
		var sprites = GetCurrentAnimationArray();
		if (sprites != null && sprites.Length > 0)
			spriteRenderer.sprite = sprites[0];

		if (lastDirection == "right")
			spriteRenderer.flipX = true;
		else if (lastDirection == "left")
			spriteRenderer.flipX = false;
	}

	Sprite[] GetCurrentAnimationArray()
	{
		if (isMoving)
		{
			if (lastDirection == "up") return upWalkSprites;
			if (lastDirection == "down") return downWalkSprites;
			if (lastDirection == "left" || lastDirection == "right") return sideWalkSprites;
		}
		else
		{
			if (lastDirection == "up") return upIdleSprites;
			if (lastDirection == "down") return downIdleSprites;
			if (lastDirection == "left" || lastDirection == "right") return sideIdleSprites;
		}
		return downIdleSprites;
	}

	public string DirToString(Vector3Int dir)
	{
		if (dir == Vector3Int.up) return "up";
		if (dir == Vector3Int.down) return "down";
		if (dir == Vector3Int.left) return "left";
		if (dir == Vector3Int.right) return "right";
		return "down";
	}

	public void LockMovement()
	{
		movementLocked = true;
		rb.linearVelocity = Vector2.zero;
		isMoving = false;
		gridDirection = Vector3Int.zero;
		bufferedDirection = Vector3Int.zero;
		SetIdleSprite();
		StopMoveSound();
	}

	public void UnlockMovement()
	{
		movementLocked = false;
	}

	public void ResetToStartPosition()
	{
		transform.position = startPosition;
		rb.linearVelocity = Vector2.zero;
		isMoving = false;
		gridDirection = Vector3Int.zero;
		bufferedDirection = Vector3Int.zero;
		targetWorldPosition = startPosition;
		lastDirection = "down";
		SetIdleSprite();
		movementLocked = true;
		StopMoveSound();
	}

	public void SetToIdle()
	{
		isMoving = false;
		gridDirection = Vector3Int.zero;
		bufferedDirection = Vector3Int.zero;
		rb.linearVelocity = Vector2.zero;
		SetIdleSprite();
		StopMoveSound();
	}

	public void OnMoveButtonDown(string dir)
	{
		if (movementLocked) return;
		mobileInputDirection = DirStringToVector(dir);
	}

	public void OnMoveButtonUp(string dir)
	{
		mobileInputDirection = Vector3Int.zero;
	}

	Vector3Int DirStringToVector(string dir)
	{
		switch (dir)
		{
			case "up": return Vector3Int.up;
			case "down": return Vector3Int.down;
			case "left": return Vector3Int.left;
			case "right": return Vector3Int.right;
		}
		return Vector3Int.zero;
	}

	void OnDrawGizmos()
	{
		if (spriteRenderer == null || wallTilemap == null) return;
		Vector3 center = spriteRenderer.bounds.center;
		Vector3 size = wallTilemap.cellSize * gizmoBoxScale;
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireCube(center, size);

		Gizmos.color = new Color(1, 0.7f, 0.1f, 0.25f);
		Vector3 areaSize = new Vector3(
			wallTilemap.cellSize.x * (areaRadius.x * 2 + 1),
			wallTilemap.cellSize.y * (areaRadius.y * 2 + 1),
			0.1f
		);
		Gizmos.DrawWireCube(center, areaSize);

		if (Application.isPlaying)
		{
			Vector3Int nextCell = wallTilemap.WorldToCell(transform.position) + gridDirection;
			Vector3 worldPos = wallTilemap.GetCellCenterWorld(nextCell);
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(worldPos, obstacleCheckRadius);
		}
	}

	// For WebGL Button Clicks (Step movement)
	public void MoveUpByButtonClick()
	{
		if (movementLocked) return;
		bufferedDirection = Vector3Int.up;
	}

	public void MoveDownByButtonClick()
	{
		if (movementLocked) return;
		bufferedDirection = Vector3Int.down;
	}

	public void MoveLeftByButtonClick()
	{
		if (movementLocked) return;
		bufferedDirection = Vector3Int.left;
	}

	public void MoveRightByButtonClick()
	{
		if (movementLocked) return;
		bufferedDirection = Vector3Int.right;
	}

	// ----------- الصوت -----------
	void PlayMoveSoundIfNeeded()
	{
		if (!isMoveSoundPlaying && !isWin)
		{
			SoundManager.Instance?.PlaySound(moveSoundKey);
			isMoveSoundPlaying = true;
		}
	}

	void StopMoveSound()
	{
		if (isMoveSoundPlaying)
		{
			SoundManager.Instance?.StopSound(moveSoundKey);
			isMoveSoundPlaying = false;
		}
	}
}
