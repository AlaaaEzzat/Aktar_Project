using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class OrientationHandler : MonoBehaviour
{
	private static OrientationHandler instance;
	public GameObject imagePanel; // Assign the image panel in the inspector

	private void Awake()
	{
		// Singleton Pattern: Ensure only one instance exists
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject); // Prevent this GameObject from being destroyed
		}
		else
		{
			Destroy(gameObject); // Destroy duplicate instances if they exist
			return;
		}
	}

	void Update()
	{
		// Check if the device is in portrait mode
		bool isPortrait = Screen.width < Screen.height;

		if (isPortrait)
		{
			imagePanel.SetActive(true); // Show the image panel
			Time.timeScale = 0f; // Pause game
			ExitFullScreen(); // Exit full-screen when in portrait mode
		}
		else
		{
			imagePanel.SetActive(false); // Hide the image panel
			Time.timeScale = 1f; // Resume game
			if (SystemInfo.deviceType == DeviceType.Handheld)
				EnterFullScreen(); // Force full-screen when in landscape mode
		}
	}

	void EnterFullScreen()
	{
		if (!Screen.fullScreen)
		{
			Screen.fullScreen = true; // Enter full-screen mode
		}
	}

	void ExitFullScreen()
	{
		if (Screen.fullScreen)
		{
			Screen.fullScreen = false; // Exit full-screen mode
		}
	}
}