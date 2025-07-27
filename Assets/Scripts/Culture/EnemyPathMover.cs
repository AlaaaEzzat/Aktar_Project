using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class EnemyPathMover : MonoBehaviour
{
	[Header("Waypoints (assign in order)")]
	public List<Transform> waypoints;

	[Header("Speed Settings")]
	public float moveSpeed = 2f; // Units per second
	public float minSpeed = 1f;  // For random speed (optional)
	public float maxSpeed = 3f;  // For random speed (optional)
	public bool randomizeSpeedOnLoop = false;

	[Header("Movement")]
	public bool loop = true;
	public bool pingPong = false; // If true, moves back and forth

	private Sequence moveSequence;

	void Start()
	{
		if (waypoints == null || waypoints.Count < 2)
		{
			Debug.LogError("EnemyPathMover needs at least 2 waypoints!");
			return;
		}
		StartMove();
	}

	void StartMove()
	{
		// Kill previous sequence if restarting
		if (moveSequence != null) moveSequence.Kill();

		moveSequence = DOTween.Sequence();

		// Optionally randomize speed each loop
		float speed = randomizeSpeedOnLoop ? Random.Range(minSpeed, maxSpeed) : moveSpeed;

		List<Transform> path = new List<Transform>(waypoints);

		if (pingPong)
		{
			// Add reversed path (without duplicating start/end)
			for (int i = waypoints.Count - 2; i > 0; i--)
				path.Add(waypoints[i]);
		}
		else if (loop)
		{
			// Add the first waypoint to end for a smooth loop
			path.Add(waypoints[0]);
		}

		// Move along the path, facing each segment
		Vector3 startPos = transform.position;
		for (int i = 0; i < path.Count; i++)
		{
			Transform target = path[i];
			Vector3 from = (i == 0) ? startPos : path[i - 1].position;
			Vector3 to = target.position;
			float duration = Vector2.Distance(from, to) / speed;

			// Flip or rotate before each move
			moveSequence.AppendCallback(() => FaceDirection(transform.position, target.position));
			moveSequence.Append(transform.DOMove(target.position, duration).SetEase(Ease.Linear));
		}

		moveSequence.OnComplete(() =>
		{
			// Loop by restarting the sequence at the current position
			if (loop)
			{
				StartMove();
			}
		});
	}

	// For dynamic speed change at runtime
	public void SetSpeed(float newSpeed)
	{
		moveSpeed = newSpeed;
		StartMove();
	}

	// Flip localScale.x for 2D sprites
	private void FaceDirection(Vector3 from, Vector3 to)
	{
		Vector3 dir = (to - from).normalized;
		if (dir.sqrMagnitude > 0.0001f)
		{
			float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
			// Y axis points toward movement: add 90 degrees if your sprite's up is Y, or 0 if it's X
			transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
			// If your sprite's Y axis is already pointing "up", use angle
			// If your sprite's X axis is pointing "right", use angle - 90
		}
	}

}
