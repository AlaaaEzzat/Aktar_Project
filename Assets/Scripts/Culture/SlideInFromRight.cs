using UnityEngine;
using DG.Tweening;

public class SlideInFromRight : MonoBehaviour
{
	public float moveDuration = 1f;    // How long the slide takes
	public float startDelay = 1f;      // Delay before sliding in

	private Vector3 originalPosition;

	void Start()
	{
		// Save the original position (where the object is in the editor)
		originalPosition = transform.position;

		// Calculate off-screen position to the right (world space)
		Vector3 screenOffRight = Camera.main.ViewportToWorldPoint(
			new Vector3(1.2f,  // 1 = right edge, >1 = off right
						Camera.main.WorldToViewportPoint(originalPosition).y,
						Mathf.Abs(Camera.main.transform.position.z - originalPosition.z))
		);
		screenOffRight.z = originalPosition.z;

		// Move object to off-screen start position
		transform.position = screenOffRight;

		// Slide to original position after delay
		DOVirtual.DelayedCall(startDelay, () =>
		{
			transform.DOMove(originalPosition, moveDuration).SetEase(Ease.OutCubic);
		});
	}
}
