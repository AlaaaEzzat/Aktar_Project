using UnityEngine;

public class DetectTransformChange : MonoBehaviour
{
	[Header("Target Image to Disable")]
	public GameObject imageToDeactivate;

	private Vector3 lastPosition;
	private Quaternion lastRotation;
	private Vector3 lastScale;

	void Start()
	{
		lastPosition = transform.position;
		lastRotation = transform.rotation;
		lastScale = transform.localScale;
	}

	void Update()
	{
		if (transform.position != lastPosition ||
			transform.rotation != lastRotation ||
			transform.localScale != lastScale)
		{
			if (imageToDeactivate != null)
			{
				imageToDeactivate.SetActive(false);
			}

			// Optional: disable this script after change is detected once
			enabled = false;
		}

		lastPosition = transform.position;
		lastRotation = transform.rotation;
		lastScale = transform.localScale;
	}
}
