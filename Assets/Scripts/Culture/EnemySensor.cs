using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySensor : MonoBehaviour
{
	[Header("Tilemap Settings")]
	public Tilemap wallTilemap;

	[Header("Detection")]
	public LayerMask enemyLayer;
	public LayerMask wallLayer;
	public LayerMask playerLayer;
	public float playerDetectDistance = 8f;

	[Header("Detection Mode")]
	[Tooltip("If true, uses Raycast Line-of-Sight (cannot see through walls). If false, uses only distance.")]
	public bool useLineOfSightDetection = true; // true = LOS, false = Circle

	[Header("Chase")]
	public float chaseDuration = 10f;
	public float chaseSpeedMultiplier = 1.4f;

	[Header("Movement")]
	public float moveSpeed = 1.1f; // tiles per second

	[Header("Sprites & Animation")]
	public SpriteRenderer spriteRenderer;
	public Sprite[] upIdleSprites, downIdleSprites, sideIdleSprites;
	public Sprite[] upWalkSprites, downWalkSprites, sideWalkSprites;
	public float walkFrameRate = 0.12f, idleFrameRate = 0.22f;

	private string lastDirection = "down";
	private int animFrame = 0;
	private float animTimer = 0f;
	private bool prevMoving = false;
	private string prevDirection = "down";

	private Vector3Int gridDirection = Vector3Int.up;
	private Vector3Int prevGridDirection = Vector3Int.up;
	private Vector3 targetWorldPosition;
	private bool isMoving = false;
	private bool movementLocked = false;

	private static readonly Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

	// Chase variables
	private bool isChasing = false;
	private float chaseTimer = 0f;
	private Transform playerTarget;
	private List<Vector3Int> currentPath = new List<Vector3Int>();
	private int pathStep = 0;
	private float normalSpeed;

	void Awake()
	{
		var cell = wallTilemap.WorldToCell(transform.position);
		transform.position = wallTilemap.GetCellCenterWorld(cell);
		targetWorldPosition = transform.position;
		SetIdleSprite();
		prevGridDirection = gridDirection;
		normalSpeed = moveSpeed;
	}

	void Update()
	{
		if (movementLocked)
		{
			AnimateSprite();
			return;
		}

		Vector3Int currCell = wallTilemap.WorldToCell(transform.position);

		// --- Player detection with mode switch ---
		if (!isChasing)
		{
			bool found = false;
			Transform foundTarget = null;

			if (useLineOfSightDetection)
			{
				// LOS: Can only see player if there's no wall between enemy and player
				Collider2D[] players = Physics2D.OverlapCircleAll(transform.position, playerDetectDistance, playerLayer);
				foreach (var playerCol in players)
				{
					if (PlayerVisible(currCell, playerCol.transform))
					{
						found = true;
						foundTarget = playerCol.transform;
						break;
					}
				}
			}
			else
			{
				// Circle mode: Detect player by proximity only
				Collider2D player = Physics2D.OverlapCircle(transform.position, playerDetectDistance, playerLayer);
				if (player != null)
				{
					found = true;
					foundTarget = player.transform;
				}
			}

			if (found)
			{
				playerTarget = foundTarget;
				StartChase();
			}
		}

		// --- Chasing logic ---
		if (isChasing)
		{
			chaseTimer -= Time.deltaTime;
			if (chaseTimer <= 0f || playerTarget == null)
			{
				EndChase();
			}
		}

		// --- Movement logic ---
		if (!isMoving)
		{
			if (isChasing && playerTarget != null)
			{
				// Follow path to player
				Vector3Int playerCell = wallTilemap.WorldToCell(playerTarget.position);
				if (currentPath.Count == 0 || pathStep >= currentPath.Count || currentPath[currentPath.Count - 1] != playerCell)
				{
					// Find new path if needed
					currentPath = FindPath(currCell, playerCell);
					pathStep = 0;
				}

				if (currentPath.Count > 1 && pathStep < currentPath.Count - 1)
				{
					Vector3Int nextCell = currentPath[pathStep + 1];
					Vector3Int moveDir = ClampDirection(nextCell - currCell);
					if (!Blocked(currCell, moveDir) && moveDir != Vector3Int.zero)
					{
						prevGridDirection = gridDirection;
						gridDirection = moveDir;
						StartStep(currCell, gridDirection);
						pathStep++;
					}
					else
					{
						// Can't move - try to recalculate next frame
						currentPath.Clear();
					}
				}
				else
				{
					// Reached player cell or no valid path
					currentPath.Clear();
				}
			}
			else
			{
				// Patrol (random movement)
				if (!Blocked(currCell, gridDirection))
				{
					StartStep(currCell, gridDirection);
				}
				else
				{
					gridDirection = PickRandomOpenDirection(currCell, OppositeDirection(gridDirection));
					TryStep(currCell);
				}
			}
		}

		AnimateSprite();
	}

	void FixedUpdate()
	{
		if (movementLocked) return;
		if (isMoving)
		{
			float moveStep = moveSpeed * wallTilemap.cellSize.x * Time.fixedDeltaTime;
			Vector2 pos = Vector2.MoveTowards(transform.position, targetWorldPosition, moveStep);
			transform.position = pos;
			if ((Vector2)transform.position == (Vector2)targetWorldPosition)
				isMoving = false;
		}
	}

	// --- Pathfinding (BFS, 4-way) ---
	List<Vector3Int> FindPath(Vector3Int start, Vector3Int end)
	{
		Queue<Vector3Int> frontier = new Queue<Vector3Int>();
		Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
		frontier.Enqueue(start);
		cameFrom[start] = start;

		while (frontier.Count > 0)
		{
			Vector3Int curr = frontier.Dequeue();
			if (curr == end) break;
			foreach (var dir in directions)
			{
				Vector3Int next = curr + dir;
				if (!cameFrom.ContainsKey(next) && !Blocked(curr, dir))
				{
					frontier.Enqueue(next);
					cameFrom[next] = curr;
				}
			}
		}

		// No path found
		if (!cameFrom.ContainsKey(end))
			return new List<Vector3Int> { start };

		// Reconstruct path
		List<Vector3Int> path = new List<Vector3Int>();
		Vector3Int currStep = end;
		while (currStep != start)
		{
			path.Add(currStep);
			currStep = cameFrom[currStep];
		}
		path.Add(start);
		path.Reverse();
		return path;
	}

	bool PlayerVisible(Vector3Int enemyCell, Transform player)
	{
		Vector3 dir = (player.position - transform.position).normalized;
		float dist = Vector3.Distance(transform.position, player.position);
		RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, wallLayer);
		return hit.collider == null;
	}

	void StartChase()
	{
		isChasing = true;
		chaseTimer = chaseDuration;
		moveSpeed = normalSpeed * chaseSpeedMultiplier;
		currentPath.Clear();
		pathStep = 0;
	}

	void EndChase()
	{
		isChasing = false;
		playerTarget = null;
		moveSpeed = normalSpeed;
		currentPath.Clear();
		pathStep = 0;
	}

	bool Blocked(Vector3Int currCell, Vector3Int dir)
	{
		if (dir == Vector3Int.zero) return true;
		var nextCell = currCell + dir;
		Vector3 checkPos = wallTilemap.GetCellCenterWorld(nextCell);
		Collider2D wall = Physics2D.OverlapPoint(checkPos, wallLayer);
		if (wall != null) return true;
		Collider2D hit = Physics2D.OverlapPoint(checkPos, enemyLayer);
		if (hit != null && hit.gameObject != gameObject) return true;
		return false;
	}

	void TryStep(Vector3Int currCell)
	{
		if (gridDirection != Vector3Int.zero && !Blocked(currCell, gridDirection))
			StartStep(currCell, gridDirection);
	}

	void StartStep(Vector3Int currCell, Vector3Int dir)
	{
		var nextCell = currCell + dir;
		targetWorldPosition = wallTilemap.GetCellCenterWorld(nextCell);
		transform.up = new Vector2(dir.x, dir.y);
		isMoving = true;
		lastDirection = DirToString(dir);
		prevGridDirection = gridDirection;
	}

	Vector3Int ClampDirection(Vector3Int dir)
	{
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			return new Vector3Int((int)Mathf.Sign(dir.x), 0, 0);
		else if (Mathf.Abs(dir.y) > 0)
			return new Vector3Int(0, (int)Mathf.Sign(dir.y), 0);
		return Vector3Int.zero;
	}

	Vector3Int PickRandomOpenDirection(Vector3Int currCell, Vector3Int excludeDir)
	{
		List<Vector3Int> openDirs = new List<Vector3Int>();
		foreach (var dir in directions)
		{
			if (dir == excludeDir) continue;
			var nextCell = currCell + dir;
			Vector3 pos = wallTilemap.GetCellCenterWorld(nextCell);
			Collider2D wall = Physics2D.OverlapPoint(pos, wallLayer);
			if (wall != null) continue;
			Collider2D hit = Physics2D.OverlapPoint(pos, enemyLayer);
			if (hit == null || hit.gameObject == gameObject)
				openDirs.Add(dir);
		}
		if (openDirs.Count > 0)
			return openDirs[Random.Range(0, openDirs.Count)];
		else
			return Vector3Int.zero;
	}

	Vector3Int OppositeDirection(Vector3Int dir)
	{
		if (dir == Vector3Int.up) return Vector3Int.down;
		if (dir == Vector3Int.down) return Vector3Int.up;
		if (dir == Vector3Int.left) return Vector3Int.right;
		if (dir == Vector3Int.right) return Vector3Int.left;
		return Vector3Int.zero;
	}

	public void LockMovement()
	{
		movementLocked = true;
		isMoving = false;
		gridDirection = Vector3Int.zero;
		SetIdleSprite();
	}

	public void UnlockMovement()
	{
		movementLocked = false;
	}

	// --- Animation Methods ---
	void AnimateSprite()
	{
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
	}

	Sprite[] GetCurrentAnimationArray()
	{
		if (isMoving)
		{
			if (lastDirection == "up") return upWalkSprites;
			if (lastDirection == "down") return downWalkSprites;
			if (lastDirection == "left" || lastDirection == "right") return sideWalkSprites;
		}
		if (lastDirection == "up") return upIdleSprites;
		if (lastDirection == "down") return downIdleSprites;
		if (lastDirection == "left" || lastDirection == "right") return sideIdleSprites;
		return downIdleSprites;
	}

	string DirToString(Vector3Int dir)
	{
		if (dir == Vector3Int.up) return "up";
		if (dir == Vector3Int.down) return "down";
		if (dir == Vector3Int.left) return "left";
		if (dir == Vector3Int.right) return "right";
		return "down";
	}
}
