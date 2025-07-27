using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Controlvideolandscape : MonoBehaviour
{
	public RawImage rawImage;
	public VideoPlayer videoPlayer;
	[SerializeField] private string videoFileName; // Set this in the Inspector
	private bool wasMutedBefore = false; // Tracks mute state before playing video
	private static bool isFirstTime = true; // Track if this is the first video play (for Android)

	private void OnEnable()
	{
		if (videoPlayer != null && rawImage != null)
		{

			if (SoundManager.Instance != null)
			{
				// Store the current mute state before muting
				wasMutedBefore = SoundManager.Instance.isMuted;
				SoundManager.Instance.MuteAll(true); // Mute sound when video starts
			}

			rawImage.enabled = true; // Show video player UI

			string videoPath = GetVideoPath();
			videoPlayer.url = videoPath; // Assign video URL
			Debug.Log("Video Path: " + videoPath);

			videoPlayer.Prepare();
			videoPlayer.prepareCompleted += OnVideoPrepared;

			// First-time Android fix: Force a simulated touch input
#if UNITY_ANDROID
            if (isFirstTime)
            {
                SimulateTouch();
                isFirstTime = false; // Mark that the first-time play is done
            }
#endif
		}
	}

	private void OnDisable()
	{
		if (videoPlayer != null)
		{
			videoPlayer.Stop(); // Stop video when disabled
			videoPlayer.prepareCompleted -= OnVideoPrepared; // Remove event listener
		}


		if (SoundManager.Instance != null)
		{
			// Restore previous mute state
			SoundManager.Instance.MuteAll(wasMutedBefore);
		}
	}

	private void OnVideoPrepared(VideoPlayer vp)
	{
		vp.Play(); // Play video as soon as it's ready
	}

	private string GetVideoPath()
	{
#if UNITY_WEBGL
		return Path.Combine(Application.streamingAssetsPath, videoFileName);
#elif UNITY_ANDROID || UNITY_IOS
            return Application.streamingAssetsPath + "/" + videoFileName;
#else
            return "file://" + Path.Combine(Application.streamingAssetsPath, videoFileName);
#endif
	}

	/// <summary>
	/// Simulates a touch on Android to allow video playback.
	/// </summary>
	private void SimulateTouch()
	{
		Debug.Log("Simulating first-time touch input for Android video playback.");
		Input.simulateMouseWithTouches = true; // Enable mouse simulation for touch
		Touch fakeTouch = new Touch();
		fakeTouch.phase = TouchPhase.Began;
		fakeTouch.position = new Vector2(Screen.width / 2, Screen.height / 2);
	}
}
