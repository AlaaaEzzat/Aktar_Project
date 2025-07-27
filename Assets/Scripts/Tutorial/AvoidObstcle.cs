using UnityEngine;
using System.Collections;

public class AvoidObstacle : MonoBehaviour
{
	[SerializeField] private GameObject sharedPanel;
	[SerializeField] private GameObject obstacle;

	private bool tutorialTriggered = false;

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (tutorialTriggered) return;

		if (other.CompareTag("Player"))
		{
			tutorialTriggered = true;

			

			if (sharedPanel) sharedPanel.SetActive(true);
			if (obstacle) obstacle.SetActive(false); // Optional: hide obstacle until ready

			Debug.Log("📘 Avoid obstacle panel shown.");
			StartCoroutine(HidePanelAfterDelay(3f)); // Wait 3 seconds then resume
		}
	}

	private IEnumerator HidePanelAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);

		if (sharedPanel) sharedPanel.SetActive(false);
		if (obstacle) obstacle.SetActive(true);


		Debug.Log("✅ Avoid obstacle tutorial ended.");
	}
}
