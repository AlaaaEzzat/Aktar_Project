using UnityEngine;

public class AlwaysOnTopCanvas : MonoBehaviour
{
	private static AlwaysOnTopCanvas instance;
	private Canvas canvas;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject); // Prevent destruction between scenes
		}
		else
		{
			Destroy(gameObject); // Destroy duplicate instances
			return;
		}

		canvas = GetComponent<Canvas>();
		canvas.sortingOrder = 100; // Ensures it's always on top
	}
}
