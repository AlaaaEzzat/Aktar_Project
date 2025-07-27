using UnityEngine;
using UnityEngine.UI;

public class CheckOrientation : MonoBehaviour
{
	public GameObject alertPanel; // UI Panel that appears in portrait mode
      // UI Text inside the panel (optional)

	private int lastScreenWidth = 0;
	private int lastScreenHeight = 0;

	void Start()
	{
		// Store initial screen size
		lastScreenWidth = Screen.width;
		lastScreenHeight = Screen.height;

#if UNITY_WEBGL && !UNITY_EDITOR
        InjectJSForOrientationCheck(); // Inject JavaScript to listen for orientation changes in WebGL
#endif

		// Initial Orientation Check
		CheckScreenOrientation();
	}

	void Update()
	{
		// Check if screen size has changed
		if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
		{
			lastScreenWidth = Screen.width;
			lastScreenHeight = Screen.height;

			Debug.Log($"?? Screen Size Updated: {Screen.width} x {Screen.height} (Orientation: {(Screen.width < Screen.height ? "Portrait" : "Landscape")})");

			CheckScreenOrientation(); // React to size change
		}
	}

	void CheckScreenOrientation()
	{
#if UNITY_WEBGL && !UNITY_EDITOR
        string jsCode = @"
            (function() {
                var isPortrait = window.innerHeight > window.innerWidth;
                console.log('Orientation: ' + (isPortrait ? 'Portrait' : 'Landscape')); // Debugging
                window.unityInstance.SendMessage('CheckOrientation', 'OnOrientationChanged', isPortrait ? '1' : '0');
            })();
        ";
        Application.ExternalEval(jsCode);
#else
		bool isPortrait = Screen.width < Screen.height;
		HandleOrientationChange(isPortrait);
#endif
	}

	public void OnOrientationChanged(string mode)
	{
		Debug.Log($"?? Orientation changed in Unity: {(mode == "1" ? "Portrait" : "Landscape")}");
		HandleOrientationChange(mode == "1");
	}

	void HandleOrientationChange(bool isPortrait)
	{
		if (isPortrait)
		{
			ShowAlertPanel();
		}
		else
		{
			HideAlertPanel();
		}

		// ?? Force Unity UI Update to Fix Display Issues
		Canvas.ForceUpdateCanvases();
	}

	void ShowAlertPanel()
	{
		Debug.Log("?? Showing Alert Panel");

		if (alertPanel != null)
		{
			alertPanel.SetActive(true);

			
		}
		else
		{
			Debug.LogError("? Alert Panel is NOT assigned in Unity Inspector!");
		}
	}

	void HideAlertPanel()
	{
		Debug.Log("? Hiding Alert Panel");

		if (alertPanel != null)
		{
			alertPanel.SetActive(false);
		}
	}

	void InjectJSForOrientationCheck()
	{
		string jsCode = @"
            (function() {
                function checkOrientation() {
                    var isPortrait = window.innerHeight > window.innerWidth;
                    window.unityInstance.SendMessage('CheckOrientation', 'OnOrientationChanged', isPortrait ? '1' : '0');
                }
                window.addEventListener('resize', checkOrientation);
                checkOrientation(); // Initial check
            })();
        ";
		Application.ExternalEval(jsCode);
	}
}
