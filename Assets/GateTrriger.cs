using System.Collections;
using UnityEngine;

public class GateTrigger : MonoBehaviour
{
	public Collider2D activeCollider; // Assign the gate's active collider in the Inspector
	public float scaleAnimationDuration = 1f; // Duration for the player shrink animation

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Player") && activeCollider != null && activeCollider.gameObject.activeSelf)
		{
			Debug.Log("?? Player reached the gate!");

			// S

			// Start the animation to shrink the player before winning
			StartCoroutine(ScalePlayerToWin(other.transform));
		}
	}

	private IEnumerator ScalePlayerToWin(Transform player)
	{
		Vector3 startScale = player.localScale;
		Vector3 targetScale = Vector3.zero;
		Vector3 targetPosition = activeCollider.bounds.center; // Center of the win collider

		float elapsedTime = 0f;

		while (elapsedTime < scaleAnimationDuration)
		{
			// Smoothly move player to the center of the collider
			player.position = Vector3.Lerp(player.position, targetPosition, elapsedTime / scaleAnimationDuration);

			// Smoothly scale player down to zero
			player.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / scaleAnimationDuration);

			elapsedTime += Time.deltaTime;
			yield return null;
		}

		// Ensure final position and scale are exactly at target
		player.position = targetPosition;
		player.localScale = targetScale;

		// Add a short delay before showing the win panel (optional)
		yield return new WaitForSeconds(0.5f);

		// Call win function from GameControllerGenerator
	}
}
