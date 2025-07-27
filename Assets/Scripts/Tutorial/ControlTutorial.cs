using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class ControlTutorial : MonoBehaviour
{
	[SerializeField] private bool inputFired = false;

	[Header("UI Panels or Buttons")]
	public GameObject mobileUI;
	public GameObject desktopUI;
	public GameObject sharedPanel;

	[DllImport("__Internal")]
	private static extern string GetUserAgent();

	void Start()
	{
		
		

#if UNITY_WEBGL && !UNITY_EDITOR
        string ua = GetUserAgent();
        Debug.Log("User Agent: " + ua);

        if (IsMobile(ua))
        {
            ShowMobileUI();
        }
        else
        {
            ShowDesktopUI();
        }
#else
		// Default to desktop in Editor
		ShowDesktopUI();
#endif
	}

	bool IsMobile(string ua)
	{
		string lowerUA = ua.ToLower();
		return lowerUA.Contains("iphone") || lowerUA.Contains("android") ||
			   lowerUA.Contains("ipad") || lowerUA.Contains("mobile");
	}

	void ShowMobileUI()
	{
		if (mobileUI) mobileUI.SetActive(true);
		if (desktopUI) desktopUI.SetActive(false);
		Debug.Log("📱 Mobile UI active.");
	}

	void ShowDesktopUI()
	{
		if (mobileUI) mobileUI.SetActive(false);
		if (desktopUI) desktopUI.SetActive(true);
		Debug.Log("🖥️ Desktop UI active.");
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || inputFired)
			sharedPanel.SetActive(false);
			
	}

	public void FireMobileInput()
	{
		Debug.Log("📲 Mobile input fired");
		inputFired = true;
	}
}
