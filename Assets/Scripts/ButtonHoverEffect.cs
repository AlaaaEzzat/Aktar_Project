using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	private Vector3 originalScale;
	public float scaleFactor = 1.1f;
	public float scaleSpeed = 0.2f;

	void Start()
	{
		originalScale = transform.localScale;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(ScaleButton(originalScale * scaleFactor));
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		StopAllCoroutines();
		StartCoroutine(ScaleButton(originalScale));
	}

	private System.Collections.IEnumerator ScaleButton(Vector3 targetScale)
	{
		float elapsedTime = 0;
		Vector3 startingScale = transform.localScale;

		while (elapsedTime < scaleSpeed)
		{
			transform.localScale = Vector3.Lerp(startingScale, targetScale, elapsedTime / scaleSpeed);
			elapsedTime += Time.deltaTime;
			yield return null;
		}

		transform.localScale = targetScale;
	}
}
