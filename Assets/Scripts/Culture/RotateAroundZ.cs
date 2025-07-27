using UnityEngine;

public class RotateAroundZ : MonoBehaviour
{
	[Header("Rotation speed in degrees per second")]
	public float rotationSpeed = 90f;

	void Update()
	{
		transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
	}
}
