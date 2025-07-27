using UnityEngine;
using UnityEngine.UI;

public class ImageFloatAndScale : MonoBehaviour
{
	[Header("Floating Settings")]
	public float floatAmplitude = 10f;
	public float floatSpeed = 1f;

	[Header("Scaling Settings")]
	public float scaleAmplitude = 0.1f;
	public float scaleSpeed = 1f;

	private Vector3 startPos;
	private Vector3 startScale;

	void Start()
	{
		startPos = transform.localPosition;
		startScale = transform.localScale;
	}

	void Update()
	{
		// Floating motion
		float floatOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
		transform.localPosition = startPos + new Vector3(0f, floatOffset, 0f);

		// Scaling motion
		float scaleOffset = Mathf.Sin(Time.time * scaleSpeed) * scaleAmplitude;
		transform.localScale = startScale + new Vector3(scaleOffset, scaleOffset, 0f);
	}
}
